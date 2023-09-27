using CoachOnline.ElasticSearch.Services;
using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.ApiObject;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Services;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
//using ITSAuth.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CoachOnline.Implementation.DataImplementation;

namespace CoachOnline.Implementation
{
    public class AdminImplementation : IAdmin
    {

        private readonly ISubscription _subscriptionSvc;
        private readonly IUser _userSvc;
        private readonly ISearch _searchSvc;
        private readonly IPlayerMedia _player;
        private readonly IUserInfo _userInfoSvc;
        private readonly IEmailApiService _emailSvc;
        private readonly ICoach _coachSvc;
        private readonly ILogger<AdminImplementation> _logger;
        private readonly IQuestionnaire _questionSvc;
        public AdminImplementation(ISubscription subscriptionSvc, IUser userSvc, ISearch searchSvc, IPlayerMedia player, IUserInfo userInfoSvc, 
            IEmailApiService emailSvc, ILogger<AdminImplementation> logger, ICoach coachSvc, IQuestionnaire questionSvc)
        {
            _subscriptionSvc = subscriptionSvc;
            _userSvc = userSvc;
            _searchSvc = searchSvc;
            _player = player;
            _userInfoSvc = userInfoSvc;
            _emailSvc = emailSvc;
            _logger = logger;
            _coachSvc = coachSvc;
            _questionSvc = questionSvc;
        }
        private async Task<int> GetAdminIdOnAuthToken(string AuthToken)
        {
            int adminId = 0;
            using (var cnx = new DataContext())
            {

                var admin = await cnx.Admins.Where(x => x.AdminLogins.Any(x => x.AuthToken == AuthToken)).FirstOrDefaultAsync();
                if (admin == null)
                {
                    throw new CoachOnlineException("Ups.. Login does not exist", CoachOnlineExceptionState.NotAuthorized);
                }

                adminId = admin.Id;
            }
            return adminId;
        }


        public async Task RemoveDrafts()
        {
            string token = await GetAdminAuthToken("admin@coachs.com", "Ziomal11!");
            using (var cnx = new DataContext())
            {
                List<int> Drafts = cnx.courses.Where(x => x.Name == "Draft").Select(x => x.Id).ToList();

                foreach (int draft in Drafts)
                {
                    await RemoveCourse(token, draft);
                }

            }
        }



        public async Task<List<ExtractAffiliateHostDataResponse>> GetAffiliatesData(DateTime? start = null, DateTime? end = null)
        {
            List<ExtractAffiliateHostDataResponse> returnData = new List<ExtractAffiliateHostDataResponse>();

            using(var ctx = new DataContext())
            {
                List<Affiliate> affiliates = null;
                if (start.HasValue && end.HasValue)
                {
                    affiliates = await ctx.Affiliates.Where(t=>t.CreationDate>= start.Value && t.CreationDate <= end.Value).ToListAsync();
                }
                else
                {
                    affiliates = await ctx.Affiliates.ToListAsync();
                }

                var groupped = affiliates.GroupBy(t => t.HostUserId).ToList();

                foreach(var aff in groupped)
                {
                    var u = await ctx.users.FirstOrDefaultAsync(t => t.Id == aff.Key && t.Status != UserAccountStatus.DELETED);

                    if(u!=null)
                    {
                        UserBillingPlan activeSub = await ctx.UserBillingPlans.Where(t => t.UserId == u.Id && t.Status == BillingPlanStatus.ACTIVE).Include(x => x.BillingPlanType).OrderByDescending(t => t.Id).FirstOrDefaultAsync();


                        string godfather = "";
                        string godfatherEmail = "";
                        string godfather2 = "";
                        string godfatherEmail2 = "";
                        bool isAff = false;                 
                        var affInfo = await ctx.Affiliates.Where(t => t.AffiliateUserId == u.Id).FirstOrDefaultAsync();
                        if (affInfo != null)
                        {
                            var gf = await ctx.users.FirstOrDefaultAsync(t => t.Id == affInfo.HostUserId && t.Status != UserAccountStatus.DELETED);
                            if (gf != null)
                            {
                                godfather = $"{gf.FirstName?.ToString()} {gf.Surname?.ToString()}";
                                godfatherEmail = gf.EmailAddress;
                                isAff = true;
                            }

                            //get host of host
                            var affInfo2 = await ctx.Affiliates.Where(t => t.AffiliateUserId == affInfo.HostUserId).FirstOrDefaultAsync();

                            if(affInfo2!=null)
                            {
                                var gf2 = await ctx.users.FirstOrDefaultAsync(t => t.Id == affInfo2.HostUserId && t.Status != UserAccountStatus.DELETED);
                                if (gf2 != null)
                                {
                                    godfather2 = $"{gf2.FirstName?.ToString()} {gf2.Surname?.ToString()}";
                                    godfatherEmail2 = gf2.EmailAddress;
               
                                }
                            }
                        }

                        var affPayments = await ctx.AffiliatePayments.Where(t => t.HostId == u.Id).ToListAsync();
                        var resp = new ExtractAffiliateHostDataResponse();
                        resp.Email = u.EmailAddress;
                        resp.Id = u.Id;
                        resp.AffiliatorType = u.AffiliatorType;
                        resp.FirstName = u.FirstName?.ToString();
                        resp.LastName = u.Surname?.ToString();
                        resp.UserType = u.UserRole.ToString();
                        resp.RegistrationDate = u.AccountCreationDate.HasValue ? u.AccountCreationDate.Value : await GetRegistrationDate(u.Id);
                        resp.SubscriptionPlan = activeSub != null ? activeSub.BillingPlanType?.Name?.ToString() : "";
                        resp.IsAffiliate = isAff ? "Yes" : "No";
                        resp.GodfatherName = godfather;
                        resp.GodfatherEmail = godfatherEmail;
                        resp.PhoneNo = u.PhoneNo?.ToString();
                        resp.TotalIncome = affPayments.Any() ? affPayments.Sum(t => t.PaymentValue) : 0;
                        resp.Currency = affPayments.Any() ? affPayments.First().PaymentCurrency : "eur";
                        resp.AffiliateType = u.UserRole == UserRoleType.COACH ? "Coach" : "Subscriber";
                        resp.Origin = await _questionSvc.UserAnswer(u.Id, QuestionnaireType.WhereAreYouFrom);
                        resp.HostGodfatherEmail = godfatherEmail2;
                        resp.HostGodfatherName = godfather2;
                        int counterAll = 0;
                        int counter2ndLine = 0;
                        int counterCoaches = 0;
                        int counterSubscribers = 0;
                        foreach(var x in aff)
                        { 
                            var exists = await ctx.users.AnyAsync(t => t.Id == x.AffiliateUserId && t.Status != UserAccountStatus.DELETED);
                            if (exists)
                            {
                                counterAll++;
                                if (x.IsAffiliateACoach)
                                {
                                    
                                    counterCoaches++;
                                }
                                else
                                {
                                    counterSubscribers++;
                                    int temp = await ctx.Affiliates.Where(t => t.HostUserId == x.AffiliateUserId).Join(ctx.users, aff => aff.AffiliateUserId, usr => usr.Id, (aff, usr) => new { Affiliate = aff, User = usr }).Where(x => x.User.Status != UserAccountStatus.DELETED).CountAsync();
                                    counterSubscribers += temp;
                                    counter2ndLine += temp;
                                }
                            }
                        }
                        resp.FirstLineAffiliatesQty = counterAll;
                        resp.SecondLineAffiliatesQty = counter2ndLine;
                        resp.CoachAffiliatesQty = counterCoaches;
                        resp.SubscribersAffiliatesQty = counterSubscribers;
                        resp.TotalAffiliatesQty = counterAll + counter2ndLine;

                        returnData.Add(resp);
                    }
                }
            }

            return returnData.Where(x=>x.TotalAffiliatesQty>0).ToList();
        }

        private async Task<DateTime?> GetRegistrationDate(int userId)
        {
            using (var ctx = new DataContext())
            {
                var data = await ctx.users.Where(t => t.Id == userId).Include(ul => ul.UserLogins).FirstOrDefaultAsync();
                if(!data.AccountCreationDate.HasValue)
                {
                    var firstLogin = data.UserLogins.OrderBy(t => t.Created).FirstOrDefault();
                    if(firstLogin==null)
                    {
                        return null;
                    }
                    else
                    {
                        return ConvertTime.FromUnixTimestamp(firstLogin.Created);
                    }
                }
                else
                {
                    return data.AccountCreationDate.Value;
                }
            }
        }

        public async Task<List<ExtractUserDataResponse>> ExtractUserData(UserRoleType? role = null, DateTime? start = null, DateTime? end = null)
        {
            List<ExtractUserDataResponse> returnData = new List<ExtractUserDataResponse>();

            using(var ctx = new DataContext())
            {
                List<User> users = null;

                if (start.HasValue && end.HasValue)
                {
                    if (role == null)
                    {
                        users = await ctx.users.Where(t => t.Status != UserAccountStatus.DELETED && 
                        (!t.AccountCreationDate.HasValue || (start.Value <= t.AccountCreationDate.Value && end.Value >= t.AccountCreationDate.Value))).ToListAsync();
                    }
                    else
                    {
                        users = await ctx.users.Where(t => t.UserRole == role.Value && t.Status != UserAccountStatus.DELETED &&
                        (!t.AccountCreationDate.HasValue || (start.Value <= t.AccountCreationDate.Value && end.Value >= t.AccountCreationDate.Value))).ToListAsync();
                    }
                }
                else
                {

                    if (role == null)
                    {
                        users = await ctx.users.Where(t => t.Status != UserAccountStatus.DELETED).ToListAsync();
                    }
                    else
                    {
                        users = await ctx.users.Where(t => t.UserRole == role.Value && t.Status != UserAccountStatus.DELETED).ToListAsync();
                    }
                }

                foreach(var u in users)
                {
                    DateTime? regDate = u.AccountCreationDate.HasValue ? u.AccountCreationDate.Value : await GetRegistrationDate(u.Id);

                    if (!start.HasValue || !end.HasValue || !regDate.HasValue || (start.HasValue && end.HasValue && regDate >= start.Value && regDate <= end.Value))
                    {

                        UserBillingPlan activeSub = await ctx.UserBillingPlans.Where(t => t.UserId == u.Id && t.Status == BillingPlanStatus.ACTIVE).Include(x => x.BillingPlanType).OrderByDescending(t => t.Id).FirstOrDefaultAsync();

                        string godfather = "";
                        string godfatherEmail = "";
                        string godfather2 = "";
                        string godfatherEmail2 = "";
                        bool isAff = false;
                        var aff = await ctx.Affiliates.Where(t => t.AffiliateUserId == u.Id).FirstOrDefaultAsync();
                        if (aff != null)
                        {
                            var gf = await ctx.users.FirstOrDefaultAsync(t => t.Id == aff.HostUserId && t.Status != UserAccountStatus.DELETED);
                            if (gf != null)
                            {
                                godfather = $"{gf.FirstName?.ToString()} {gf.Surname?.ToString()}";
                                godfatherEmail = gf.EmailAddress;
                                isAff = true;
                            }

                            //host of host
                            var aff2 = await ctx.Affiliates.Where(t => t.AffiliateUserId == aff.HostUserId).FirstOrDefaultAsync();
                            if(aff2!=null)
                            {
                                var gf2 = await ctx.users.FirstOrDefaultAsync(t => t.Id == aff2.HostUserId && t.Status != UserAccountStatus.DELETED);
                                if (gf2 != null)
                                {
                                    godfather2 = $"{gf2.FirstName?.ToString()} {gf2.Surname?.ToString()}";
                                    godfatherEmail2 = gf2.EmailAddress;
                                }
                            }
                        }


                        var resp = new ExtractUserDataResponse();
                        resp.Email = u.EmailAddress;
                        resp.Id = u.Id;
                        resp.AffiliatorType = u.AffiliatorType;
                        resp.FirstName = u.FirstName?.ToString();
                        resp.LastName = u.Surname?.ToString();
                        resp.UserType = u.UserRole.ToString();
                        resp.RegistrationDate = regDate;
                        resp.SubscriptionPlan = activeSub != null ? activeSub.BillingPlanType?.Name?.ToString() : "";
                        resp.IsAffiliate = isAff ? "Yes" : "No";
                        resp.GodfatherName = godfather;
                        resp.GodfatherEmail = godfatherEmail;
                        resp.HostGodfatherEmail = godfatherEmail2;
                        resp.HostGodfatherName = godfather2;
                        resp.PhoneNo = u.PhoneNo?.ToString();
                        resp.Origin = await _questionSvc.UserAnswer(u.Id, QuestionnaireType.WhereAreYouFrom);
                        string libraryName = null;
                        if(u.UserRole == UserRoleType.INSTITUTION_STUDENT && u.InstitutionId.HasValue)
                        {
                            var lib = await ctx.LibraryAccounts.FirstOrDefaultAsync(l => l.Id == u.InstitutionId.Value);
                            libraryName = lib.LibraryName;
                        }

                        resp.LibraryName = libraryName;

                        returnData.Add(resp);

                    }
                }
            }

            return returnData;
        }

        public async Task UpdateUserBillingInfo(UpdateUserBillingInfoAsAdminRequest request)
        {

            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);

            if (!DataImplementation.ValidateBankAccount(request.BankAccountNumber))
            {
                throw new CoachOnlineException($"Account number {request.BankAccountNumber} is not valid IBAN.", CoachOnlineExceptionState.DataNotValid);
            }

            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == request.UserId)
                    .Include(x => x.companyInfo)
                    .FirstOrDefault();

                CheckExist(user, "User");

                if (user.companyInfo == null)
                {
                    user.companyInfo = new CompanyInfo();
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    user.companyInfo.Name = request.Name;
                }

                if (!string.IsNullOrEmpty(request.City))
                {
                    user.companyInfo.City = request.City;
                }

                if (!string.IsNullOrEmpty(request.SiretNumber))
                {
                    user.companyInfo.SiretNumber = request.SiretNumber;
                }

                if (!string.IsNullOrEmpty(request.BankAccountNumber))
                {
                    user.companyInfo.BankAccountNumber = request.BankAccountNumber;
                }
                if (!string.IsNullOrEmpty(request.RegisterAddress))
                {
                    user.companyInfo.RegisterAddress = request.RegisterAddress;
                }
                if (!string.IsNullOrEmpty(request.Country))
                {
                    user.companyInfo.Country = request.Country;
                }
                if (!string.IsNullOrEmpty(request.VatNumber))
                {
                    user.companyInfo.VatNumber = request.VatNumber;
                }

                if (!string.IsNullOrEmpty(request.PostCode))
                {
                    user.companyInfo.ZipCode = request.PostCode;
                }

                if (!string.IsNullOrEmpty(request.BICNumber))
                {
                    var pattern = @"^[a-zA-Z0-9]+$";
                    if (!Regex.IsMatch(request.BICNumber, pattern) || request.BICNumber.Length <8 || request.BICNumber.Length > 11)
                    {
                        _logger.LogInformation("Invalid BIC number");
                        throw new CoachOnlineException("Le numéro BIC doit comporter un minimum de 8 caractères, un maximum de 11 et peut comprendre des chiffres et des lettres.", CoachOnlineExceptionState.DataNotValid);
                    }
                    user.companyInfo.BICNumber = request.BICNumber;
                }

                await cnx.SaveChangesAsync();

            }
        }


        public async Task<string> UpdateCoachPhoto(UpdateCoachPhotoAsAdminRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            string oldAvatar = "";
            string generateName = null;
            if (request.RemoveAvatar)
            {
                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).FirstOrDefault();
                    CheckExist(user, "User");
                    oldAvatar = user.AvatarUrl;
                    user.AvatarUrl = null;
                    await cnx.SaveChangesAsync();
                }

            }
            else
            {

                generateName = $"{Statics.LetsHash.RandomHash($"{DateTime.Now}")}";
                SaveImage(request.Base64Photo, generateName);
                generateName = generateName + ".jpg";


                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).FirstOrDefault();
                    CheckExist(user, "User");
                    oldAvatar = user.AvatarUrl;
                    user.AvatarUrl = generateName;
                    await cnx.SaveChangesAsync();
                }
            }
            if (!string.IsNullOrEmpty(oldAvatar))
            {
                try
                {
                    RemoveFile(oldAvatar, FileType.Image);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }

            return generateName;


        }

        public async Task<string> UpdateCoachCV(UpdateCoachCVAsAdminRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            string oldCV = "";
            string generateName = null;

            var fileData = request.FileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var extension = "";
            if (fileData.Length >= 2)
            {
                extension = fileData.Last();
            }

            if (request.RemoveCV)
            {
                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).FirstOrDefault();
                    CheckExist(user, "User");
                    if (user.UserCV != null) {
                        oldCV = user.UserCV.DocumentUrl;
                        user.UserCV.DocumentUrl = null;
                        await cnx.SaveChangesAsync();
                    }
                }
            }
            else
            {

                generateName = $"{Statics.LetsHash.RandomHash($"{DateTime.Now}")}";
                // Extensions.SaveDocumentAsync(request.Base64Photo, generateName, extension);
                SaveDocumentsFile(generateName, extension, request.Base64Photo);
                generateName = generateName + "." + extension;
                // SaveImage(request.Base64Photo, generateName);

                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).FirstOrDefault();
                    CheckExist(user, "User");
                    
                    if (user.UserCV == null)
                    {
                        oldCV = null;
                        user.UserCV = new UserDocument();
                        user.UserCV.DocumentUrl = generateName;
                    }
                    else 
                    {
                        oldCV = user.UserCV.DocumentUrl;
                        user.UserCV.DocumentUrl = generateName;
                    }
                    await cnx.SaveChangesAsync();
                }
            }
            if (!string.IsNullOrEmpty(oldCV))
            {
                try
                {
                    RemoveFile(oldCV, FileType.Document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }

            return generateName;
        }

        public async Task<List<string>> UpdateCoachReturns(UpdateCoachReturnsAsAdminRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            string oldReturns = "";
            string generateName = null;
            List<string> filesName = new List<string>();

            var fileData = request.FileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var extension = "";
            if (fileData.Length >= 2)
            {
                extension = fileData.Last();
            }

            if (request.RemoveReturn)
            {
                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId)
                        .Include(x => x.UserReturns)
                        .FirstOrDefault();
                    CheckExist(user, "User");

                    if (user.UserReturns != null) {
                        foreach (var doc in user.UserReturns)
                        {
                            if (doc.DocumentUrl != request.FileName) {
                                filesName.Add(doc.DocumentUrl);
                            }
                            // if (doc.DocumentUrl == request.FileName) {
                            //     oldReturns = doc.DocumentUrl;
                            // }
                        }
                        // if (!string.IsNullOrEmpty(oldReturns))
                        // {
                        oldReturns = request.FileName;
                        var docToRemove = cnx.UserDocuments.Where(x => x.DocumentUrl == request.FileName).FirstOrDefault();
                        user.UserReturns.Remove(docToRemove);
                        await cnx.SaveChangesAsync();
                        // }
                    }
                }
            }
            else
            {

                generateName = $"{Statics.LetsHash.RandomHash($"{DateTime.Now}")}";
                SaveDocumentsFile(generateName, extension, request.Base64Photo);
                generateName = generateName + "." + extension;
            
                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).Include(x => x.UserReturns).FirstOrDefault();
                    CheckExist(user, "User");
                    
                    if (user.UserReturns == null)
                    {
                        oldReturns = null;
                        user.UserReturns = new List<UserDocument>();
                        user.UserReturns.Add(new UserDocument()
                        {
                            DocumentUrl = generateName,
                        });
                    }
                    else 
                    {
                        oldReturns = null;
                        user.UserReturns.Add(new UserDocument()
                        {
                            DocumentUrl = generateName,
                        });
                    }

                    foreach (var doc in user.UserReturns)
                    {
                        filesName.Add(doc.DocumentUrl);
                    }

                    await cnx.SaveChangesAsync();
                }
            }
            if (!string.IsNullOrEmpty(oldReturns))
            {
                try
                {
                    RemoveFile(oldReturns, FileType.Document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }

            return filesName;
        }

        public async Task<List<string>> UpdateCoachAttestations(UpdateCoachAttestationAsAdminRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            string oldAttestations = "";
            string generateName = null;
            List<string> filesName = new List<string>();

            var fileData = request.FileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var extension = "";
            if (fileData.Length >= 2)
            {
                extension = fileData.Last();
            }

            if (request.RemoveAttestation)
            {
                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId)
                        .Include(x => x.UserAttestations)
                        .FirstOrDefault();
                    CheckExist(user, "User");

                    if (user.UserAttestations != null) {
                        foreach (var doc in user.UserAttestations)
                        {
                            if (doc.DocumentUrl != request.FileName) {
                                filesName.Add(doc.DocumentUrl);
                            }
                            // if (doc.DocumentUrl == request.FileName) {
                            //      oldAttestations = request.FileName;
                            // }
                        }
                        // if (!string.IsNullOrEmpty(oldAttestations))
                        // {
                        oldAttestations = request.FileName;
                        var docToRemove = cnx.UserDocuments.Where(x => x.DocumentUrl == request.FileName).FirstOrDefault();
                        user.UserAttestations.Remove(docToRemove);
                        await cnx.SaveChangesAsync();
                        // }
                    }
                }
            }
            else
            {

                generateName = $"{Statics.LetsHash.RandomHash($"{DateTime.Now}")}";
                SaveDocumentsFile(generateName, extension, request.Base64Photo);
                generateName = generateName + "." + extension;

                using (var cnx = new DataContext())
                {
                    var user = cnx.users.Where(x => x.Id == request.UserId).Include(x => x.UserAttestations).FirstOrDefault();
                    CheckExist(user, "User");
                    
                    if (user.UserAttestations == null)
                    {
                        oldAttestations = null;
                        user.UserAttestations = new List<UserDocument>();
                        user.UserAttestations.Add(new UserDocument()
                        {
                            DocumentUrl = generateName,
                        });
                    }
                    else 
                    {
                        oldAttestations = null;
                        user.UserAttestations.Add(new UserDocument()
                        {
                            DocumentUrl = generateName,
                        });
                    }

                    foreach (var doc in user.UserAttestations)
                    {
                        filesName.Add(doc.DocumentUrl);
                    }

                    await cnx.SaveChangesAsync();
                }
            }
            if (!string.IsNullOrEmpty(oldAttestations))
            {
                try
                {
                    RemoveFile(oldAttestations, FileType.Document);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cant remove file " + e.Message);
                }
            }

            return filesName;
        }

        public async Task UpdateUserProfile(UpdateUserProfileAsAdminRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == request.UserId)
                   //category
                   //.Include(x => x.AccountCategory)
                   .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(request.City))
                {
                    user.City = request.City;
                }

                if (!string.IsNullOrEmpty(request.Country))
                {
                    user.Country = request.Country;
                }

                if (!string.IsNullOrEmpty(request.PhoneNo))
                {
                    user.PhoneNo = request.PhoneNo;
                }

                if (!string.IsNullOrEmpty(request.Adress))
                {
                    user.Adress = request.Adress;
                }

                if (!string.IsNullOrEmpty(request.PostalCode))
                {
                    user.PostalCode = request.PostalCode;
                }

                if (!string.IsNullOrEmpty(request.Name))
                {
                    user.FirstName = request.Name;
                }

                if (!string.IsNullOrEmpty(request.Surname))
                {
                    user.Surname = request.Surname;
                }
                if (!string.IsNullOrEmpty(request.Gender))
                {
                    user.Gender = request.Gender;
                }
                if (!string.IsNullOrEmpty(request.Bio))
                {
                    user.Bio = request.Bio;
                }

                if (request.YearOfBirth.HasValue)
                {

                    if (request.YearOfBirth <= 1900)
                    {
                        request.YearOfBirth = 1900;
                    }
                    else if (request.YearOfBirth >= DateTime.Now.Year)
                    {
                        request.YearOfBirth = DateTime.Now.Year;
                    }
                    user.YearOfBirth = request.YearOfBirth;

                }

                await cnx.SaveChangesAsync();


          


            }
        }


        public async Task AssignChildToCategory(AssignDismissChildRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);

            if (request.ChildCategoryId == request.ParentCategoryId)
            {
                throw new CoachOnlineException("You can add parent as child.", CoachOnlineExceptionState.CantChange);
            }


            using (var cnx = new DataContext())
            {
                var parent = cnx.courseCategories.Where(x => x.Id == request.ParentCategoryId)
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .FirstOrDefault();

                if (parent != null)
                {
                    if (parent.Parent != null)
                    {
                        throw new CoachOnlineException("You can't add actual child as parent.", CoachOnlineExceptionState.CantChange);
                    }

                    var child = cnx.courseCategories.Where(x => x.Id == request.ChildCategoryId)
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .FirstOrDefault();

                    if (child.Children.Count > 0)
                    {
                        throw new CoachOnlineException($"{child.Name} is currently Parent. You can assign it as child", CoachOnlineExceptionState.CantChange);
                    }
                    if (child.Parent != null)
                    {
                        throw new CoachOnlineException($"{child.Name} has already parent. You need to design it from {child.Parent.Id} : {child.Parent.Name}", CoachOnlineExceptionState.CantChange);

                    }

                    parent.Children.Add(child);

                    await cnx.SaveChangesAsync();

                   

                }
                else
                {
                    throw new CoachOnlineException("Parent category does not exist.", CoachOnlineExceptionState.NotExist);
                }

            }
        }

        public async Task DeleteAttachment(int attachmentId)
        {
            using(var ctx = new DataContext())
            {
                var attachment = await ctx.EpisodeAttachments.Where(t => t.Id == attachmentId).FirstOrDefaultAsync();
                attachment.CheckExist("Attachment");
                var filename = $"{attachment.Hash}.{attachment.Extension}";
                Console.WriteLine($"Removing file {filename}");
                RemoveFile(filename, FileType.Attachment);

                ctx.EpisodeAttachments.Remove(attachment);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateUserPassword(int userId, string pass, string repeat)
        {
            if(pass != repeat)
            {
                throw new CoachOnlineException("Passwords don't match", CoachOnlineExceptionState.PasswordsNotMatch);
            }

            if(string.IsNullOrEmpty(pass))
            {
                throw new CoachOnlineException("Password cannot be empty", CoachOnlineExceptionState.DataNotValid);
            }

            using(var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId && t.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if(user.SocialLogin.HasValue && user.SocialLogin.Value)
                {
                    throw new CoachOnlineException("Cannot update user password because the user is authenticated by external social account.", CoachOnlineExceptionState.CantChange);
                }

                var encrypted = LetsHash.ToSHA512(pass);

                user.Password = encrypted;

                await ctx.SaveChangesAsync();
            }
        }

        private async Task SaveAttachmentFile(string Hash, string Extension, string FileData)
        {
            byte[] data = await Task.Run(() =>
            {
                return Convert.FromBase64String(FileData);
            });
            await File.WriteAllBytesAsync($"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/{Hash}.{Extension}", data);
        }

        private async Task SaveDocumentsFile(string Hash, string Extension, string FileData)
        {
            byte[] data = await Task.Run(() =>
            {
                return Convert.FromBase64String(FileData);
            });
            await File.WriteAllBytesAsync($"{ConfigData.Config.EnviromentPath}/wwwroot/documents/{Hash}.{Extension}", data);
        }


        public async Task AddAttachment(int episodeId, string attachmentName, string extension, string attachmentBase64)
        {
            using (var ctx = new DataContext())
            {
                var ep = await ctx.Episodes.FirstOrDefaultAsync(t => t.Id == episodeId);
                ep.CheckExist("Episode");
                string filename = LetsHash.RandomHash(DateTime.Now.ToString());
                await SaveAttachmentFile(filename, extension, attachmentBase64);
                EpisodeAttachment a = new EpisodeAttachment();
                a.EpisodeId = episodeId;
                a.Name = attachmentName;
                a.Hash = filename;
                a.Extension = extension;
                a.Added = ConvertTime.ToUnixTimestampLong(DateTime.Now);

                ctx.EpisodeAttachments.Add(a);
                await ctx.SaveChangesAsync();
                
            }
        }

        public async Task DismissChildFromCategory(AssignDismissChildRequest request)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);

            if (request.ChildCategoryId == request.ParentCategoryId)
            {
                throw new CoachOnlineException("You can add parent as child.", CoachOnlineExceptionState.CantChange);
            }


            using (var cnx = new DataContext())
            {
                var parent = cnx.courseCategories.Where(x => x.Id == request.ParentCategoryId)
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .FirstOrDefault();

                if (parent != null)
                {


                    var child = cnx.courseCategories.Where(x => x.Id == request.ChildCategoryId)
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .FirstOrDefault();

                    if (child == null)
                    {
                        throw new CoachOnlineException($"Child with id {request.ChildCategoryId} does not exist.", CoachOnlineExceptionState.CantChange);

                    }

                    if (!parent.Children.Any(x => x.Id == child.Id))
                    {
                        throw new CoachOnlineException($"{child.Name} is currently Parent. You can assign it as child", CoachOnlineExceptionState.CantChange);
                    }


                    parent.Children.Remove(child);

                    await cnx.SaveChangesAsync();

               

                }
                else
                {
                    throw new CoachOnlineException("Parent category does not exist.", CoachOnlineExceptionState.NotExist);
                }

            }
        }

        public async Task UpdateCategoryFamily(UpdateCategoryFamilyRequest request)
        {
            throw new CoachOnlineException("Method UpdateCategoryFamily is deprecated. Use Assign/Dismiss ChildFromCategory instead.", CoachOnlineExceptionState.Deprecated);
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: request.AdminAuthToken);
            if (request.Children.Any(x => x == request.CategoryId))
            {
                throw new CoachOnlineException("You can add parent as child.", CoachOnlineExceptionState.CantChange);
            }

            using (var cnx = new DataContext())
            {
                var parent = cnx.courseCategories.Where(x => x.Id == request.CategoryId)
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .FirstOrDefault();
                if (parent != null)
                {
                    if (parent.Parent != null)
                    {
                        throw new CoachOnlineException("You can't add actual child as parent.", CoachOnlineExceptionState.CantChange);
                    }

                    List<Category> childs = cnx.courseCategories.Where(x => request.Children.Any(y => x.Id == y))
                        .Include(x => x.Children)
                        .Include(x => x.Parent)
                        .ToList();

                    var childsHasParents = childs.Where(x => x.Parent != null).FirstOrDefault();
                    if (childsHasParents != null)
                    {
                        throw new CoachOnlineException($"Category with Id {childsHasParents.Id} and name {childsHasParents.Name} is already connected with other parent ({childsHasParents.Parent.Id} : {childsHasParents.Parent.Name})", CoachOnlineExceptionState.CantChange);
                    }

                    foreach (var c in childs)
                    {
                        c.AdultOnly = parent.AdultOnly;
                    }


                    parent.Children = childs;
                    await cnx.SaveChangesAsync();


                

                }
                else
                {
                    throw new CoachOnlineException("Parent category does not exist.", CoachOnlineExceptionState.NotExist);
                }

            }

        }

        private async Task RemoveEpisodesFromContinueLearning(int episodeId)
        {
            using(var ctx = new DataContext())
            {
              
                var eps = await ctx.StudentOpenedEpisodes.Where(t => t.EpisodeId == episodeId).ToListAsync();

                foreach(var ep in eps)
                {       
                    ctx.StudentOpenedEpisodes.Remove(ep);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        private async Task RemoveCourseFromContinueLearning(int courseId)
        {
            using (var ctx = new DataContext())
            {

                var courses = await ctx.StudentOpenedCourses.Where(t => t.CourseId == courseId).ToListAsync();

                foreach (var c in courses)
                {
                    ctx.StudentOpenedCourses.Remove(c);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task RemoveCourse(string AdminAuthToken, int CourseId)
        {
            int AdminId = await GetAdminIdOnAuthToken(AuthToken: AdminAuthToken);


            using (var cnx = new DataContext())
            {
                var course = await cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.Episodes).ThenInclude(a=>a.Attachments)
                    .Include(x => x.RejectionsHistory)
                    .FirstOrDefaultAsync();

                CheckExist(course, "Course");


                foreach (var episode in course.Episodes)
                {
                    //await RemoveEpisodeFromCourse(AdminAuthToken, CourseId, episode.Id);

                    foreach(var attachment in episode.Attachments)
                    {
                        RemoveFile($"{attachment.Hash}.{attachment.Extension}", FileType.Attachment);
                    }

                    RemoveFile(episode.MediaId, FileType.Video);
                    await RemoveEpisodesFromContinueLearning(episode.Id);
                }

                if(!string.IsNullOrEmpty(course.PhotoUrl))
                {
                    RemoveFile(course.PhotoUrl, FileType.Image);
                }
                //List<Rejection> rejections = new List<Rejection>();

                //foreach (var r in course.RejectionsHistory)
                //{
                //    rejections.Add(r);
                //}

                //foreach (var r in rejections)
                //{
                //    course.RejectionsHistory.Remove(r);
                //}


                cnx.courses.Remove(course);


                await cnx.SaveChangesAsync();

                await RemoveCourseFromContinueLearning(CourseId);

                // await _searchSvc.DeleteCourse(CourseId);
                // await _searchSvc.ReaddCategory(course.CategoryId);
                // await _searchSvc.ReaddCoach(course.UserId);
                await _searchSvc.ReindexAll();

            }
        }

        public async Task<CourseResponse> GetCourse(int courseId)
        {
            List<CourseResponse> data = new List<CourseResponse>();
            using (var ctx = new DataContext())
            {
                var c = await ctx.courses.Where(t => t.Id == courseId).FirstOrDefaultAsync();
                if (c != null)
                {
                    CourseResponse courseResponse = new CourseResponse();
                    courseResponse.Category = await _userInfoSvc.GetCourseCategory(c.CategoryId);
                    courseResponse.Created = c.Created;
                    courseResponse.Description = c.Description;
                    courseResponse.Id = c.Id;
                    courseResponse.Name = c.Name ?? "";
                    courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                    courseResponse.State = c.State;
                    courseResponse.Episodes = await _userInfoSvc.GetCourseEpisodes(c.Id);
                    courseResponse.Coach = await _userInfoSvc.GetCoachData(c.UserId, true, true);
                    courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                    courseResponse.Prerequisite = c.Prerequisite ?? "";
                    courseResponse.Objectives = c.Objectives ?? "";
                    courseResponse.PublicTargets = c.PublicTargets ?? "";
                    courseResponse.CertificationQCM = c.CertificationQCM ?? "";

                    return courseResponse;

                }

                return null;
            }

        }

        public async Task ChangeCourseState(string AdminAuthToken, int CourseId, CourseState state)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var course = await cnx.courses.Where(x => x.Id == CourseId).FirstOrDefaultAsync();
                CheckExist(course, $"Course with it {CourseId}");
                if (course.State == state)
                {
                    throw new CoachOnlineException($"Course already has state {state}", CoachOnlineExceptionState.AlreadyChanged);
                }
                course.State = state;
                await cnx.SaveChangesAsync();

            }
        }


        public async Task<int> CreateCategory(string AdminAuthToken, string CategoryName)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            int categoryNewId = 0;
            using (var cnx = new DataContext())
            {
                var categoryExist = cnx.courseCategories.Where(x => x.Name.ToLower() == CategoryName.ToLower()).FirstOrDefault();
                if (categoryExist != null)
                {
                    throw new CoachOnlineException("Category with name {CategoryName} already exist.", CoachOnlineExceptionState.AlreadyExist);
                }
                Category category = new Category
                {
                    Name = CategoryName
                };
                cnx.courseCategories.Add(category);
                await cnx.SaveChangesAsync();
                categoryNewId = category.Id;
            }

           

            return categoryNewId;

        }



        public async Task<GetAdminCategoriesResponse> GetCategories(GetAdminCategoriesRequest request)
        {
            int adminId = await GetAdminIdOnAuthToken(request.AdminAuthToken);

            GetAdminCategoriesResponse response = new GetAdminCategoriesResponse();
            response.items = new List<GetAdminCategoriesItem>();

            using (var cnx = new DataContext())
            {
                var cats = cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Select(x => x).ToList();
                foreach (var c in cats)
                {
                    if (c.Parent == null)
                    {
                        var catToAdd = new GetAdminCategoriesItem
                        {
                            Id = c.Id,
                            Name = c.Name ?? "",

                        };


                        if (c.Children != null)
                        {
                            catToAdd.Children = new List<GetAdminCategoriesFamily>();
                            foreach (var cc in c.Children)
                            {
                                catToAdd.Children.Add(new GetAdminCategoriesFamily { Id = cc.Id, Name = cc.Name });
                            }
                        }
                        response.items.Add(catToAdd);
                    }
                }
            }

            return response;

        }

        public async Task UpdateCategoryName(string AdminAuthToken, int CategoryId, string CategoryNameNew)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var category = await cnx.courseCategories.Where(x => x.Id == CategoryId).FirstOrDefaultAsync();
                CheckExist(category, "Category");

                category.Name = CategoryNameNew;
                await cnx.SaveChangesAsync();

            
            }

        }


        public async Task RemoveCategory(string AdminAuthToken, int Categoryid)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var categoryToRemove = await cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Where(x => x.Id == Categoryid).FirstOrDefaultAsync();
                CheckExist(categoryToRemove, "Category you are trying to remove");

                var containsCourses = await cnx.courses.AnyAsync(t => t.CategoryId == categoryToRemove.Id);

                if(containsCourses)
                {
                    _logger.LogError($"You cannot delete category {categoryToRemove.Name} as there are courses assigned. Firstly change category for courses which are assigned this category.");
                    throw new CoachOnlineException("Vous ne pouvez pas supprimer cette catégorie car des cours sont attribués.  Changez d'abord de catégorie pour les cours auxquels cette catégorie est attribuée.", CoachOnlineExceptionState.CantChange);
                    
                }

                if (categoryToRemove.Parent != null)
                {
                    var parent = await cnx.courseCategories
                    .Include(x => x.Children)
                    .Include(x => x.Parent)
                    .Where(x => x.Id == categoryToRemove.Parent.Id).FirstOrDefaultAsync();

                    parent.Children.Remove(categoryToRemove);

                }

                if (categoryToRemove.Children.Count != 0)
                {
                    throw new CoachOnlineException("This Category has children. You can't remove that before you did not dismiss children.", CoachOnlineExceptionState.CantChange);
                }

              

                cnx.courseCategories.Remove(categoryToRemove);
                try
                {
                    await cnx.SaveChangesAsync();

                 
                }
                catch (DbUpdateException e)
                {
                    throw new CoachOnlineException("Can't remove category its propably connected with some objects.", CoachOnlineExceptionState.PermissionDenied);
                }

            }
        }


        public async Task RemoveEpisodeFromCourse(string AdminAuthToken, int CourseId, int LessonId)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var Course = cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.Episodes)
                    .FirstOrDefault();


                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == LessonId).FirstOrDefault();
                CheckExist(Lesson, "Episode");

                if (Lesson.Attachments != null && Lesson.Attachments.Count > 0)
                {
                    foreach (var a in Lesson.Attachments)
                    {
                        Lesson.Attachments.Remove(a);
                    }
                }


                Course.Episodes.Remove(Lesson);
                await cnx.SaveChangesAsync();
            }
        }


        public async Task RemoveAttachmentFromCourse(string AdminAuthToken, int CourseId, int EpisodeId, int AttachmentId)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var Course = cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.Episodes)
                    .FirstOrDefault();

                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExist(Lesson, "Episode");

                var Attachment = Lesson.Attachments.Where(x => x.Id == AttachmentId).FirstOrDefault();
                CheckExist(Attachment, "Attachment");

                try
                {
                    RemoveFile(Attachment.Hash, FileType.Attachment);

                }
                catch (Exception e)
                {

                }

                Lesson.Attachments.Remove(Attachment);

                await cnx.SaveChangesAsync();
            }
        }


        public async Task RemoveMediaFromEpisode(string AdminAuthToken, int CourseId, int EpisodeId)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var Course = cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)
                    .Include(x => x.Episodes)
                    .FirstOrDefault();

                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExist(Lesson, "Episode");

                try
                {

                    RemoveFile(Lesson.MediaId, FileType.Video);
                }
                catch (Exception e)
                {

                }

                Lesson.MediaId = "";
                await cnx.SaveChangesAsync();

            }
        }


        public async Task<string> GetAdminAuthToken(string email, string password)
        {
            password = LetsHash.ToSHA512(password);
            string token = "";
            using (var cnx = new DataContext())
            {
                var admin = await cnx.Admins.Where(x => x.Email == email)
                    .Include(x => x.AdminLogins)
                    .FirstOrDefaultAsync();
                if (admin == null)
                {
                    throw new CoachOnlineException("Password incorrect.", CoachOnlineExceptionState.WrongPassword);

                }
                if (admin.Password != password)
                {
                    throw new CoachOnlineException("Password incorrect", CoachOnlineExceptionState.WrongPassword);
                }

                token = LetsHash.RandomHash($"{DateTime.Now}");

                admin.AdminLogins.Add(new Model.AdminLogin
                {
                    AuthToken = token,
                    LoggedIn = DateTime.Now
                });

                await cnx.SaveChangesAsync();

            }

            return token;
        }

        private async Task<SubscriptionInfoAPIResponse> FillSubscriptionData(UserBillingPlan ubp)
        {
            SubscriptionInfoAPIResponse data = new SubscriptionInfoAPIResponse();
            data.ExpiryDate = ubp.ExpiryDate;
            data.SubscriptionId = ubp.Id;
            data.StartDate = ubp.ActivationDate;
            data.PlannedStartDate = ubp.PlannedActivationDate;
            data.StudentOption = ubp.IsStudent;
            data.SubscriptionName = ubp.BillingPlanType.Name;
            data.SubscriptionStatusStr = ubp.StatusStr;
            data.SubscriptionStatus = ubp.Status;
            data.Price = ubp.BillingPlanType.Price.Amount.Value;
            data.Period = $"{ubp.BillingPlanType.Price.Period} {ubp.BillingPlanType.Price.PeriodType}";
            data.Currency = ubp.BillingPlanType.Price.Currency;
            data.UserId = ubp.UserId;

            if(data.StudentOption)
            {
                data.StudentSubscriptionStatus = new StudentOptionInfoAPIRespponse();
                data.StudentSubscriptionStatus.StudentCardStatus = ubp.StudentCardVerificationStatus;
                data.StudentSubscriptionStatus.StudentCardStatusStr = ubp.StudentCardVerificationStatusStr;
          

                using(var ctx = new DataContext())
                {
                    var studentCardImgs = await ctx.UserBillingPlans.Where(t => t.Id == ubp.Id).Include(x => x.StudentCardData).FirstOrDefaultAsync();

                    if(studentCardImgs.StudentCardData.Any())
                    {
                        data.StudentSubscriptionStatus.StudentCardData = new List<StudentCardImg>();
                        foreach (var sc in studentCardImgs.StudentCardData)
                        {
                            var img = new StudentCardImg();
                            img.PhotoName = sc.StudentsCardPhotoName;
                            img.PhotoUrl = sc.StudentCardUrl;

                            data.StudentSubscriptionStatus.StudentCardData.Add(img);
                        }
                    }
                }
            }
            
            return data;
        }

        public async Task<UserSubscriptionAPIReponse> GetUserSubscriptionData(int userId)
        {
            
            var subResp = new UserSubscriptionAPIReponse();
            try
            {
                var active = await _subscriptionSvc.GetUserActiveSubscriptionPlan(userId);
                var current = await _subscriptionSvc.GetUserCurrentSubscriptionPlan(userId);

                var usr = await _userSvc.GetUserById(userId);
                var invoices = await _subscriptionSvc.GetUserInvoices(usr);
                if (active.Id == current.Id)
                {
                    subResp.ActiveSubscription = await FillSubscriptionData(active);
                }
                else
                {
                    subResp.ActiveSubscription = await FillSubscriptionData(active);
                    subResp.AwaitingSubscription = await FillSubscriptionData(current);
                }

                subResp.Invoices = invoices;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            

            return subResp;
        }

        public async Task<UserAPI> GetUserData(int userId)
        {
            using(var ctx = new DataContext())
            {
                var user = await ctx.users
                    .Where(x => x.Id == userId)
                    .Include(x => x.companyInfo)
                    .Include(x => x.AccountCategories)
                    //category

                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Category)
                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.Episodes)
                    .ThenInclude(x => x.Attachments)

                    .Include(x => x.OwnedCourses)
                    .ThenInclude(x => x.RejectionsHistory)
                    .Include(x => x.UserCV)
                    .Include(x => x.UserAttestations)
                    .Include(x => x.UserReturns)
                    .OrderBy(x => x.Id)
                    .FirstOrDefaultAsync();

                if(user == null)
                {
                    throw new CoachOnlineException("User does not exist", CoachOnlineExceptionState.NotExist);
                }
                UserAPI userApi = UserToUserApi(user);

                var lastOpenedCourses = await _player.LastOpenedCourses(userId);
                userApi.LastOpenedCourses = lastOpenedCourses != null ? lastOpenedCourses.ToList() : null;
                userApi.QuestionnaireResponse = await _questionSvc.UserAnswer(userId, QuestionnaireType.WhereAreYouFrom);
                return userApi;
            }
        }

        public async Task<GetUsersResponse> GetUsers(string authToken, int count, int lastId, bool fromOldest, UserRoleType? byRole = null)
        {
            GetUsersResponse response = new GetUsersResponse();

            List<UserAPI> users = new List<UserAPI>();
            int adminId = await GetAdminIdOnAuthToken(authToken);
            if (count == 0)
            {
                count = int.MaxValue;
            }

            List<CoachSummarizedRankingReponse> coachRank = null;
            if(!byRole.HasValue || byRole.Value == UserRoleType.COACH)
            {
                coachRank = await _coachSvc.GetCurrentCoachesRankingAll();
            }

            using (var cnx = new DataContext())
            {
                List<Model.User> dirtyList = new List<Model.User>();

                var totalCount = await cnx.users.Select(x => x.Id).ToListAsync();
                response.TotalCount = totalCount.Count();

                if (fromOldest)
                {

                    if (lastId == 0)
                    {
                        lastId = int.MaxValue;
                    }
                    dirtyList = await cnx.users
                        .Where(x => x.Status != UserAccountStatus.DELETED && x.Id < lastId && (!byRole.HasValue || byRole.Value == x.UserRole))
                        .Include(x => x.companyInfo)
                        .Include(x => x.AccountCategories)
                        //category

                        .Include(x => x.OwnedCourses)
                        .ThenInclude(x => x.Category)
                        .Include(x => x.OwnedCourses)
                        .ThenInclude(x => x.Episodes)
                        .ThenInclude(x => x.Attachments)

                        .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.RejectionsHistory)
                        
                        .Include(x => x.UserCV)
                        .Include(x => x.UserAttestations)
                        .Include(x => x.UserReturns)
                       
                        .OrderBy(x => x.Id)
                        .Take(count)
                        .ToListAsync();
                }
                else
                {


                    dirtyList = await cnx.users
                       .Where(x => x.Status != UserAccountStatus.DELETED && x.Id > lastId && (!byRole.HasValue || byRole.Value == x.UserRole))
                       .Include(x => x.companyInfo)
                       .Include(x => x.AccountCategories)
                       //category
                       .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.Category)
                       .ThenInclude(x => x.Parent)
                       .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.Category)
                       .ThenInclude(x => x.Children)

                       .Include(x => x.OwnedCourses)

                       .ThenInclude(x => x.Episodes)
                       .ThenInclude(x => x.Attachments)

                       .Include(x => x.OwnedCourses)
                       .ThenInclude(x => x.RejectionsHistory)
                        
                       .Include(x => x.UserCV)
                       .Include(x => x.UserAttestations)
                       .Include(x => x.UserReturns)

                       .OrderByDescending(x => x.Id)
                       .Take(count)
                       .ToListAsync();
                }


                foreach (var u in dirtyList)
                {

                    var userApi = UserToUserApi(u);
                    if(u.UserRole == UserRoleType.COACH && coachRank != null)
                    {
                        var rank = coachRank.FirstOrDefault(x => x.CoachId == u.Id);
                        if(rank != null)
                        {
                            userApi.RankPosition = rank.RankPosition;
                            userApi.TotalMinutes = rank.TotalMinutes;
                        }
                    }

                    userApi.QuestionnaireResponse = await _questionSvc.UserAnswer(u.Id, QuestionnaireType.WhereAreYouFrom);
                   // var lastOpenedCourses = await _player.LastOpenedCourses(u.Id);
                   // userApi.LastOpenedCourses = lastOpenedCourses != null ? lastOpenedCourses.ToList() : null;
                    users.Add(userApi);
                }

            }
            response.Users = users;

            return response;
        }

        public async Task ChangeUserState(string AdminAuthToken, int UserId, UserAccountStatus userAccountStatus)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.Where(x => x.Id == UserId).Include(x=>x.UserLogins).FirstOrDefaultAsync();
                CheckExist(user, "User");

                if ( user.Status != UserAccountStatus.CONFIRMED && user.Status == userAccountStatus)
                {
                    throw new CoachOnlineException($"Users state is already {user.Status}", CoachOnlineExceptionState.CantChange);
                }

                 if(userAccountStatus == UserAccountStatus.DELETED)
                {
                    throw new CoachOnlineException($"Wrong user state", CoachOnlineExceptionState.CantChange);
                }
                else if(userAccountStatus == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION)
                {
                    user.EmailConfirmed = false;
                }
                else
                {
                    user.Status = userAccountStatus;
                    user.EmailConfirmed = true;
                }
                await cnx.SaveChangesAsync();

                if(user.Status == UserAccountStatus.BANNED)
                {
                    var now = ConvertTime.ToUnixTimestampLong(DateTime.Now);
                    var tokensStillvalid = user.UserLogins.Where(t => t.ValidTo >= now && t.Disposed==false).ToList();

                    if(tokensStillvalid.Any())
                    {
                        foreach(var validToken in tokensStillvalid)
                        {
                            var token = await cnx.userLogins.FirstOrDefaultAsync(t => t.Id == validToken.Id);
                            if(token != null)
                            {
                                token.Disposed = true;
                            }
                        }

                        await cnx.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task AcceptSuggestedCategory(int suggestedCatId)
        {
            using(var ctx = new DataContext())
            {
                var newCat = await ctx.PendingCategories.FirstOrDefaultAsync(x => x.Id == suggestedCatId);
                newCat.CheckExist("Category");

                newCat.State = PendingCategoryState.APPROVED;

                var addCat = new Category();
                addCat.Name = newCat.CategoryName;
                addCat.ParentId = newCat.ParentId;
                addCat.AdultOnly = newCat.AdultOnly;

                ctx.courseCategories.Add(addCat);

                await ctx.SaveChangesAsync();

                var user = await _userSvc.GetUserById(newCat.CreatedByUserId);

                if (user != null && System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/AcceptCategorySuggestion.html"))
                {
                    string body = "";
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/AcceptCategorySuggestion.html");
                    body = body.Replace("###CATEGORY_NAME###", newCat.CategoryName);
               
                    body = body.Replace("###PLATFORM_URL###", $"{Statics.ConfigData.Config.WebUrl}");

                    await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage()
                    {
                        AuthorEmail = "info@coachs-online.com",
                        AuthorName = "Coachs-Online",
                        Topic = "Coachs-Online notification - votre catégorie a été acceptée",
                        Body = body,
                        ReceiverEmail = user.EmailAddress,
                        ReceiverName = user.FirstName != null ? $"{user.FirstName} {user.Surname}" : user.EmailAddress
                    });
                }
            }
        }

        public async Task RejectSuggestedCategory(int suggestedCatId, string rejectReason)
        {
            using (var ctx = new DataContext())
            {
                var newCat = await ctx.PendingCategories.FirstOrDefaultAsync(x => x.Id == suggestedCatId);
                newCat.CheckExist("Category");

                newCat.State = PendingCategoryState.REJECTED;
                newCat.RejectReason = rejectReason;

                await ctx.SaveChangesAsync();

                var user = await _userSvc.GetUserById(newCat.CreatedByUserId);
              
                if (user!= null && System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/RejectCategorySuggestion.html"))
                {
                    string body = "";
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/RejectCategorySuggestion.html");
                    body = body.Replace("###CATEGORY_NAME###", newCat.CategoryName);
                    body = body.Replace("###REJECT_REASON###", newCat.RejectReason);
                    body = body.Replace("###PLATFORM_URL###", $"{Statics.ConfigData.Config.WebUrl}");

                    await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage()
                    {
                        AuthorEmail = "info@coachs-online.com",
                        AuthorName = "Coachs-Online",
                        Topic = "Coachs-Online notification - votre catégorie a été rejetée",
                        Body = body,
                        ReceiverEmail = user.EmailAddress,
                        ReceiverName = user.FirstName != null ? $"{user.FirstName} {user.Surname}" : user.EmailAddress
                    }); 
                }
            }
        }

        public async Task<List<SuggestedCategoryResponse>> GetCategoriesSuggestedByUsers()
        {
            var data = new List<SuggestedCategoryResponse>();
            using(var ctx = new DataContext())
            {
                var cats = await ctx.PendingCategories.Where(t => t.State == PendingCategoryState.PENDING).Include(u=>u.CreatedByUser).ToListAsync();
                foreach(var c in cats)
                {
                    var resp = new SuggestedCategoryResponse();
                    resp.Id = c.Id;
                    resp.Name = c.CategoryName;
                    resp.CoachId = c.CreatedByUserId;
                    resp.CoachName = $"{c.CreatedByUser.FirstName} {c.CreatedByUser.Surname}";
                    resp.CoachEmail = c.CreatedByUser.EmailAddress;
                    resp.ParentId = c.ParentId;
                    resp.AdultOnly = c.AdultOnly;
                    if(c.ParentId.HasValue)
                    {
                        var cat = await ctx.courseCategories.FirstOrDefaultAsync(t => t.Id == c.ParentId.Value);
                        if(cat != null)
                        {
                            resp.ParentName = cat.Name;
                            resp.ParentsChildren = await GetChildCategories(c.ParentId.Value);

                        }
                    }

                    data.Add(resp);
                }

            }

            return data;
        }

        public async Task<List<CategoryAPI>> GetChildCategories(int categoryId)
        {
            var cats = new List<CategoryAPI>();

            using (var ctx = new DataContext())
            {
                var parent = await ctx.courseCategories.Where(t => t.Id == categoryId).Include(c => c.Children).FirstOrDefaultAsync();
                if(parent.Children != null)
                {
                    foreach(var c in parent.Children)
                    {
                        var cat = new CategoryAPI();
                        cat.Id = c.Id;
                        cat.Name = c.Name;
                        cat.ParentId = parent.Id;
                        cat.ParentName = parent.Name;
                        cat.AdultOnly = c.AdultOnly;
                        cats.Add(cat);
                    }
                }
            }

            return cats;
        }

        public async Task<GetCoursesAsAdminResponse> GetCoursesWithUsers(GetCoursesAsAdminRequest request)
        {
            GetCoursesAsAdminResponse response = new GetCoursesAsAdminResponse();
            response.ResponsePairs = new List<GetCoursesAsAdminResponseData>();
            int adminId = await GetAdminIdOnAuthToken(request.AdminAuthToken);
            List<Course> courses = new List<Course>();
            if (request.Count == 0)
            {
                request.Count = int.MaxValue;
            }
            using (var cnx = new DataContext())
            {
                var allIdsTotalCoursesCount = await cnx.courses.Where(x => x.State == CourseState.PENDING).Select(x => x.Id).ToListAsync();
                response.TotalCoursesCount = allIdsTotalCoursesCount.Count();
                if (request.FromOldest)
                {
                    if (request.LastId == 0)
                    {
                        request.LastId = int.MaxValue;
                    }
                    courses = cnx.courses.Where(x => x.Id < request.LastId)
                        .Include(x => x.Category)
                        .ThenInclude(x => x.Parent)
                        .Include(x => x.Category)
                        .ThenInclude(x => x.Children)
                        .Include(x => x.Episodes)
                        .ThenInclude(x => x.Attachments)
                        .Include(x => x.RejectionsHistory)
                        .Where(x => request.IncludeAll ? x.Id > 0 : x.State == CourseState.PENDING)
                         .Take(request.Count)
                         .OrderByDescending(x => x.Id)
                         .ToList();
                }
                else
                {

                    courses = cnx.courses.Where(x => x.Id > request.LastId)
                        .Include(x => x.Episodes)
                        .ThenInclude(x => x.Attachments)
                        .Where(x => request.IncludeAll ? x.Id > 0 : x.State == CourseState.PENDING)
                        .Include(x => x.RejectionsHistory)

                         .Take(request.Count)
                         .OrderBy(x => x.Id)
                         .ToList();
                }

                foreach (var c in courses)
                {
                    var user = await cnx.users

                        .Where(x => x.Id == c.UserId)
                        .FirstOrDefaultAsync();

                    UserShortAPI userShortAPI = new UserShortAPI();

                    if (user != null)
                    {
                        userShortAPI.Email = user.EmailAddress ?? "";
                        userShortAPI.Id = user.Id;
                        userShortAPI.FirstName = user.FirstName ?? "";
                        userShortAPI.LastName = user.Surname ?? "";
                        userShortAPI.Address = user.Adress ?? "";
                        userShortAPI.City = user.City ?? "";
                        userShortAPI.Country = user.Country ?? "";
                        userShortAPI.Gender = user.Gender ?? "";
                        userShortAPI.PhoneNo = user.PhoneNo ?? "";
                        userShortAPI.PhotoUrl = user.AvatarUrl ?? "";
                        userShortAPI.YearOfBirth = user.YearOfBirth;
                        userShortAPI.Status = user.Status;
                        userShortAPI.PostalCode = user.PostalCode ?? "";
                        userShortAPI.RegistrationDate = user.AccountCreationDate.HasValue? user.AccountCreationDate : GetFirstLogInDate(user.Id);
                       
                    }

                    CourseAPI course = CourseToCourseAPI(c);
                    GetCoursesAsAdminResponseData responseData = new GetCoursesAsAdminResponseData
                    {
                        Course = course,
                        User = userShortAPI
                    };
                    if (c.RejectionsHistory != null && c.RejectionsHistory.Count > 0)
                    {
                        responseData.LastDeclineReason = c.RejectionsHistory.FirstOrDefault().Reason;
                        responseData.LastDeclineDate = c.RejectionsHistory.FirstOrDefault().Date;
                    }
                    else
                    {
                        responseData.LastDeclineReason = "";
                        responseData.LastDeclineDate = 0;
                    }
                    response.ResponsePairs.Add(responseData);
                }
            }


            return response;
        }


        private CourseAPI CourseToCourseAPI(Course course)
        {
            CourseAPI courseAPI = new CourseAPI();

            CategoryAPI categoryResponse = new CategoryAPI();
            categoryResponse.ParentsChildren = new List<CategoryAPI>();
            if (course.Category != null)
            {
                categoryResponse.Name = course.Category.Name ?? "";
                categoryResponse.Id = course.Category.Id;
                if (course.Category.Parent != null)
                {
                    categoryResponse.ParentId = course.Category.Parent.Id;
                    categoryResponse.ParentName = course.Category.Parent.Name ?? "";
                    if (course.Category.Parent.Children != null && course.Category.Parent.Children.Count > 0)
                    {
                        foreach (var pc in course.Category.Parent.Children)
                        {
                            categoryResponse.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                        }
                    }
                }
            }

            if (course.Category != null)
            {

                courseAPI.Category = categoryResponse;
            }
            else
            {
                courseAPI.Category = null;
            }
            courseAPI.Created = course.Created;
            courseAPI.Description = course.Description ?? "";
            courseAPI.Id = course.Id;
            courseAPI.Name = course.Name ?? "";
            courseAPI.PhotoUrl = course.PhotoUrl ?? "";
            courseAPI.State = course.State;
            courseAPI.HasPromo = course.HasPromo.HasValue && course.HasPromo.Value;
            courseAPI.BannerPhotoUrl = course.BannerPhotoUrl ?? "";
            if (course.Episodes != null)
            {
                courseAPI.Episodes = new List<EpisodeAPI>();
                foreach (var courseEpisode in course.Episodes)
                {
                    courseAPI.Episodes.Add(EpisodeToEpisodeAPI(courseEpisode));
                }
            }

            return courseAPI;
        }

        private EpisodeAPI EpisodeToEpisodeAPI(Episode episode)
        {
            EpisodeAPI episodeAPI = new EpisodeAPI();
            if (episode.Attachments != null)
            {

                episodeAPI.Attachments = episode.Attachments;
            }

            episodeAPI.Created = episode.Created;
            episodeAPI.Description = episode.Description ?? "";
            episodeAPI.Id = episode.Id;
            episodeAPI.MediaId = episode.MediaId ?? "";
            episodeAPI.OrdinalNumber = episode.OrdinalNumber;
            episodeAPI.Title = episode.Title ?? "";
            episodeAPI.needConversion = episode.MediaNeedsConverting;
            episodeAPI.Length = episode.MediaLenght;
            episodeAPI.IsPromo = episode.IsPromo.HasValue && episode.IsPromo.Value;
            episodeAPI.EpisodeState = episode.EpisodeState;
            return episodeAPI;
        }

        private CompanyInfoAPI CompanyInfoToCompanyInfoAPI(CompanyInfo companyInfo)
        {
            CompanyInfoAPI companyInfoAPI = new CompanyInfoAPI();
            companyInfoAPI.BankAccountNumber = companyInfo.BankAccountNumber ?? "";
            companyInfoAPI.City = companyInfo.City ?? "";
            companyInfoAPI.Country = companyInfo.Country ?? "";
            companyInfoAPI.Name = companyInfo.Name ?? "";
            companyInfoAPI.RegisterAddress = companyInfo.RegisterAddress ?? "";
            companyInfoAPI.SiretNumber = companyInfo.SiretNumber ?? "";
            companyInfoAPI.VatNumber = companyInfo.VatNumber ?? "";
            companyInfoAPI.PostalCode = companyInfo.ZipCode;
            companyInfoAPI.BICNumber = companyInfo.BICNumber;

            return companyInfoAPI;
        }

        private string GetProfessionName(int professionId)
        {
            using(var ctx = new DataContext())
            {
                return ctx.Professions.FirstOrDefault(t => t.Id == professionId)?.Name;
            }
        }

        private DateTime? GetFirstLogInDate(int user)
        {
            using(var ctx = new DataContext())
            {
                var u = ctx.users.Where(t => t.Id == user).Include(l => l.UserLogins).FirstOrDefault();

                if(u.AccountCreationDate.HasValue)
                {
                    return u.AccountCreationDate.Value;
                }
                if (u.UserLogins.Any())
                {
                    var firstLogin = u.UserLogins.OrderBy(t => t.Created).FirstOrDefault();

                    return ConvertTime.FromUnixTimestamp(firstLogin.Created);
                }

                return null;
            }
        }

        private UserAPI UserToUserApi(User user)
        {

    
            UserAPI userApi = new UserAPI();

            userApi.Bio = user.Bio ?? "";
            userApi.City = user.City ?? "";
            userApi.Email = user.EmailAddress ?? "";
            userApi.PhoneNo = user.PhoneNo ?? "";
            userApi.FirstName = user.FirstName ?? "";
            userApi.Gender = user.Gender ?? "";
            userApi.Id = user.Id;
            userApi.Status = user.Status == UserAccountStatus.CONFIRMED && !user.EmailConfirmed? UserAccountStatus.AWAITING_EMAIL_CONFIRMATION: user.Status;
            userApi.LastName = user.Surname ?? "";
            userApi.YearOfBirth = user.YearOfBirth;
            userApi.PhotoUrl = user.AvatarUrl ?? "";
            userApi.UserRole = user.UserRole;
            userApi.PostalCode = user.PostalCode;
            userApi.Country = user.Country;
            userApi.Address = user.Adress;
            userApi.ProfessionId = user.ProfessionId;
            userApi.ProfessionName = user.ProfessionId.HasValue ? GetProfessionName(user.ProfessionId.Value) : "";
            userApi.UserSubscriptionStatus = user.SubscriptionActive ? "ACTIVE" : "NOT ACTIVE";
            userApi.SocialLogin = user.SocialLogin.HasValue && user.SocialLogin.Value;
            userApi.EmailConfirmed = user.EmailConfirmed;
            userApi.RegistrationDate = user.AccountCreationDate.HasValue ? user.AccountCreationDate.Value : GetFirstLogInDate(user.Id);
            userApi.TrialEndDate = userApi.RegistrationDate.HasValue? userApi.RegistrationDate.Value.AddDays(3) : new DateTime(2021,10,1).AddDays(3);
            userApi.TrialActive = userApi.TrialEndDate >= DateTime.Now;
            userApi.AffiliatorType = user.AffiliatorType;
            if (userApi.UserRole == UserRoleType.COACH)
            {
                userApi.PaymentEnabled = user.PaymentsEnabled;
                userApi.WithdrawalsEnabled = user.WithdrawalsEnabled;
                if (user.AccountCategories != null)
                {
                    userApi.UserCategories = new List<CategoryAPI>();
                    foreach(var cat in user.AccountCategories)
                    {
                        userApi.UserCategories.Add(new CategoryAPI() { AdultOnly = cat.AdultOnly, Id = cat.Id, Name = cat.Name, ParentId = cat.ParentId});
                    }
                }
            }
            else
            {
                userApi.PaymentEnabled = null;
                userApi.WithdrawalsEnabled = null;
            }


            if (user.companyInfo != null)
            {
                userApi.companyInfo = CompanyInfoToCompanyInfoAPI(user.companyInfo);
            }

            if (user.OwnedCourses != null)
            {
                userApi.OwnedCourses = new List<CourseAPI>();
                foreach (var c in user.OwnedCourses)
                {

                    userApi.OwnedCourses.Add(CourseToCourseAPI(c));
                }
            }

            userApi.UserDocuments = new CoachInfoDocumentAPI();
            // if (user.UserCV != null || user.UserAttestations != null || user.UserReturns != null) {
            if (user.UserCV != null) {
                userApi.UserDocuments.UserCV = user.UserCV.DocumentUrl;
            }
            if (user.UserReturns != null) {
                userApi.UserDocuments.Returns= new List<string>();
                foreach (var c in user.UserReturns)
                {
                    userApi.UserDocuments.Returns.Add(c.DocumentUrl);
                }
            }
            if (user.UserAttestations != null) {
                userApi.UserDocuments.Diplomas= new List<string>();
                foreach (var c in user.UserAttestations)
                {
                    userApi.UserDocuments.Diplomas.Add(c.DocumentUrl);
                }
            }
            // }
            return userApi;
        }

        public async Task RejectCourse(string AdminAuthToken, int CourseId, string Reason)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);
            using (var cnx = new DataContext())
            {
                var course = await cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.RejectionsHistory)
                    .FirstOrDefaultAsync();
                CheckExist(course, "Course");

                course.State = CourseState.REJECTED;
                course.RejectionsHistory.Add(new Rejection
                {
                    Date = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    Reason = Reason ?? ""
                });

                await cnx.SaveChangesAsync();

                await RemoveCourseFromContinueLearning(CourseId);

                // await _searchSvc.DeleteCourse(CourseId);
                // await _searchSvc.ReaddCategory(course.CategoryId);
                // await _searchSvc.ReaddCoach(course.UserId);

                await _searchSvc.DeleteCourse(CourseId);
                await _searchSvc.ReaddCategory(course.CategoryId);
                await _searchSvc.ReaddCoach(course.UserId);
            }
        }


        public async Task AcceptCourse(string AdminAuthToken, int CourseId)
        {
            int adminId = await GetAdminIdOnAuthToken(AdminAuthToken);

            using (var cnx = new DataContext())
            {
                var course = await cnx.courses.Where(x => x.Id == CourseId)
                    .Include(x => x.RejectionsHistory)
                    .FirstOrDefaultAsync();
                CheckExist(course, "Course");

                course.State = CourseState.APPROVED;
                course.PublishedCount = course.PublishedCount.HasValue ? course.PublishedCount.Value + 1 : 1;
                await cnx.SaveChangesAsync();

                await SendEmailConfirmationAboutPublishedCourse(CourseId);
            }
        }


        public async Task SendEmailConfirmationAboutPublishedCourse(int courseId)
        {

            using (var cnx = new DataContext())
            {
                var course = await cnx.courses.Where(x => x.Id == courseId).Include(u=>u.User)
                    .FirstOrDefaultAsync();
                CheckExist(course, "Course");

               if(course.State == CourseState.APPROVED && course.User != null && course.PublishedCount.HasValue && course.PublishedCount.Value == 1)
                {
                    string body = "";
                    if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/CourseIsPublic.html"))
                    {
                        body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/CourseIsPublic.html");
                        body = body.Replace("###PLATFORM_URL###", $"{Statics.ConfigData.Config.WebUrl}/course?id={course.Id}");
                        body = body.Replace("###COURSE_NAME###", $"{course.Name}");
                    }

                    if (body != "")
                    {
                        await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage
                        {
                            AuthorEmail = "info@coachs-online.com",
                            AuthorName = "Coachs-online",
                            Body = body,
                            ReceiverEmail = course.User.EmailAddress,
                            ReceiverName = $"{course.User.FirstName?.ToString()} {course.User.Surname?.ToString()}",
                            Topic = $" Le cours {course.Name} a été publié."
                        });
                    }
                }


            }
        }

        public async Task UpdateEpisodeInCourse(string AuthToken, int CourseId, int EpisodeId, string Title, string Description, int OrdinalNumber)
        {
            int adminId = await GetAdminIdOnAuthToken(AuthToken);
            using (var cnx = new DataContext())
            {


                var Course = cnx.courses
                    .Include(x => x.Episodes)
                    .Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExist(Lesson, "Episode");
                if (!string.IsNullOrEmpty(Title))
                {
                    Lesson.Title = Title ?? "";
                }
                if (!string.IsNullOrEmpty(Description))
                {
                    Lesson.Description = Description ?? "";
                }

                if (OrdinalNumber != 0)
                {
                    Lesson.OrdinalNumber = OrdinalNumber;
                }

                await cnx.SaveChangesAsync();


            }
        }


        // public async Task UpdateCourseDetailsAsync(string AuthToken, int CourseId, string Name, int Category, string Description, string PhotoUrl)
        public async Task UpdateCourseDetailsAsync(UpdateCourseAdminRequest request)
        {
            int adminId = await GetAdminIdOnAuthToken(request.AdminAuthToken);
            using (var cnx = new DataContext())
            {

                var course = cnx.courses.Where(x => x.Id == request.CourseId).FirstOrDefault();
                if (course == null)
                {
                    throw new CoachOnlineException("Course does not exist.", CoachOnlineExceptionState.PermissionDenied);
                }
                if (!string.IsNullOrEmpty(request.Name))
                {
                    course.Name = request.Name;
                }
                if (request.Category != 0)
                {
                    var category = await cnx.courseCategories.Where(x => x.Id == request.Category).FirstOrDefaultAsync();
                    CheckExist(category, "Category");
                    course.Category = category;
                }
                if (!string.IsNullOrEmpty(request.Description))
                {
                    course.Description = request.Description;
                }
                if (!string.IsNullOrEmpty(request.PhotoUrl))
                {
                    course.PhotoUrl = request.PhotoUrl;
                }

                if (!string.IsNullOrEmpty(request.Prerequisite))
                {
                    course.Prerequisite = request.Prerequisite;
                }
                if (!string.IsNullOrEmpty(request.Objectives))
                {
                    course.Objectives = request.Objectives;
                }
                if (!string.IsNullOrEmpty(request.PublicTargets))
                {
                    course.PublicTargets = request.PublicTargets;
                }
                if (!string.IsNullOrEmpty(request.CertificationQCM))
                {
                    course.CertificationQCM = request.CertificationQCM;
                }

                await cnx.SaveChangesAsync();

             
            }
        }



        public async Task UpdateEpisodeMedia(string AuthToken, int CourseId, int EpisodeId)
        {
            int adminId = await GetAdminIdOnAuthToken(AuthToken);

            using (var cnx = new DataContext())
            {


                var Course = cnx.courses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExist(Lesson, "Episode");

                // Lesson.MediaId = MediaHashId;
                await cnx.SaveChangesAsync();
            }
        }


        public async Task UpdateEpisodeAttachment(string AuthToken, int CourseId, int EpisodeId, string AttachmentHashId)
        {
            int adminId = await GetAdminIdOnAuthToken(AuthToken);

            using (var cnx = new DataContext())
            {


                var Course = cnx.courses.Where(x => x.Id == CourseId).FirstOrDefault();
                CheckExist(Course, "Course");
                var Lesson = Course.Episodes.Where(x => x.Id == EpisodeId).FirstOrDefault();
                CheckExist(Lesson, "Episode");

                Lesson.MediaId = AttachmentHashId;

                //Lesson.Attachments = AttachmentHashId;
                await cnx.SaveChangesAsync();
            }
        }
        private void CheckExist(object obj, string FieldName)
        {
            if (obj == null)
            {
                //Log.Error($"{FieldName} does not exist.");
                throw new CoachOnlineException($"{FieldName} does not exist.", CoachOnlineExceptionState.NotExist);
            }
        }

        public async Task FlagCourses(List<CourseFlagRqs> coursesToFlag)
        {
            if(!coursesToFlag.Any())
            {
                return;
            }

            coursesToFlag.OrderBy(t => t.OrederNo).ThenBy(c => c.CourseId);

            using (var ctx = new DataContext())
            {
             
                int order = 1;
                foreach(var f in coursesToFlag)
                {
                    var exists = await ctx.courses.AnyAsync(t => t.Id == f.CourseId);
                    if (exists)
                    {
                        var c = new FlaggedCourse();
                        c.CourseId = f.CourseId;
                        c.OrderNo = order;
                        c.CreationDate = DateTime.Now;
                        order++;

                        ctx.FlaggedCourses.Add(c);
                    }
                }

                var toRemove = await ctx.FlaggedCourses.ToListAsync();

                ctx.FlaggedCourses.RemoveRange(toRemove);

                await ctx.SaveChangesAsync();
            }

        }

        private async Task<bool> CheckCourseIsFlagged(int courseId)
        {
            using (var ctx = new DataContext())
            {
                var c = await ctx.FlaggedCourses.FirstOrDefaultAsync(t => t.CourseId == courseId);
                if(c!=null)
                {
                    return true;
                }

                return false;
            }
        }

        public async Task<List<CourseResponse>> GetCoursesToFlag()
        {
            List<CourseResponse> courses = new List<CourseResponse>();
            using (var ctx = new DataContext())
            {
                var coursers = await ctx.courses.Where(t => t.State == CourseState.APPROVED).Include(c => c.Category).ThenInclude(p => p.Parent).Include(e => e.Episodes).ThenInclude(m => m.Attachments).ToListAsync();

                foreach (var c in coursers)
                {
                    CourseResponse courseResponse = new CourseResponse();
                    courseResponse.Episodes = new List<EpisodeResponse>();
                    courseResponse.RejectionsHistory = new List<RejectionResponse>();
                    courseResponse.Category = new CategoryAPI();
                    courseResponse.Category.ParentsChildren = new List<CategoryAPI>();
                    courseResponse.IsFlagged = await CheckCourseIsFlagged(c.Id);
                    courseResponse.Created = c.Created;
                    courseResponse.Description = c.Description;
                    courseResponse.Id = c.Id;
                    courseResponse.Name = c.Name ?? "";
                    courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                    courseResponse.State = c.State;
                    courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                    courseResponse.Coach = new CoachInfoResponse();

                    var courseUser = await ctx.courses.Where(t => t.Id == c.Id).Include(u => u.User).ThenInclude(x => x.OwnedCourses).ThenInclude(cat => cat.Category).FirstOrDefaultAsync();
                    if (courseUser != null)
                    {
                        courseResponse.Coach.Bio = courseUser.User.Bio;
                        courseResponse.Coach.Country = courseUser.User.Country;
                        courseResponse.Coach.Email = courseUser.User.EmailAddress;
                        courseResponse.Coach.FirstName = courseUser.User.FirstName;
                        courseResponse.Coach.LastName = courseUser.User.Surname;
                        courseResponse.Coach.Id = courseUser.UserId;
                        courseResponse.Coach.YearOfBirth = courseUser.User.YearOfBirth;
                        courseResponse.Coach.Gender = courseUser.User.Gender;
                        courseResponse.Coach.UserCategories = await _player.GetCoachCategories(courseUser.UserId);
                        courseResponse.Coach.PhotoUrl = courseUser.User.AvatarUrl;
                        courseResponse.Coach.Courses = new List<CourseResponse>();

                        foreach (var cr in courseUser.User.OwnedCourses)
                        {
                            // if (cr.Id != c.Id)
                            //{
                            if (cr.State == CourseState.APPROVED)
                            {
                                courseResponse.Coach.Courses.Add(new CourseResponse()
                                {
                                    Description = cr.Description,
                                    Category = new CategoryAPI() { Id = cr.CategoryId, Name = cr.Category.Name, AdultOnly = cr.Category.AdultOnly },
                                    Name = cr.Name,
                                    PhotoUrl = cr.PhotoUrl,
                                    Id = cr.Id,
                                    Created = cr.Created,
                                    State = cr.State

                                });
                            }
                            //}
                        }
                    }

                    if (c.Episodes != null && c.Episodes.Count > 0)
                    {
                        foreach (var e in c.Episodes)
                        {
                            EpisodeResponse episodeResponse = new EpisodeResponse();

                            episodeResponse.Created = e.Created;
                            episodeResponse.Description = e.Description ?? "";
                            episodeResponse.Id = e.Id;
                            episodeResponse.MediaId = e.MediaId;
                            episodeResponse.OrdinalNumber = e.OrdinalNumber;
                            episodeResponse.Title = e.Title ?? "";
                            episodeResponse.CourseId = e.CourseId;
                            episodeResponse.Length = e.MediaLenght;
                            episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                            episodeResponse.Attachments = await _player.GetEpisodeAttachments(e.Id);
                            episodeResponse.EpisodeState = e.EpisodeState;
                            courseResponse.Episodes.Add(episodeResponse);



                        }
                    }

                    if (c.Category != null)
                    {
                        courseResponse.Category.Name = c.Category.Name;
                        courseResponse.Category.Id = c.Category.Id;
                        courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                        if (c.Category.Parent != null)
                        {
                            courseResponse.Category.ParentId = c.Category.Parent.Id;
                            courseResponse.Category.ParentName = c.Category.Parent.Name;
                            if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                            {
                                foreach (var pc in c.Category.Parent.Children)
                                {
                                    courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                                }
                            }
                        }
                    }

                    courses.Add(courseResponse);
                }

                return courses;

            }
        }

        public async Task<UploadCoursePhotoResponse> UploadCoursePhoto(string PhotoBase, int CourseId)
        {
            var resp = new UploadCoursePhotoResponse();
            using (var cnx = new DataContext())
            {

                var course = await cnx.courses.FirstOrDefaultAsync(t => t.Id == CourseId);
                course.CheckExist("Course");

                string generateName = Statics.LetsHash.RandomHash($"{DateTime.Now}");
                await Task.Run(() => SaveImage(PhotoBase, generateName));
                var imageUrl = $"images/{generateName}.jpg";

                course.PhotoUrl = imageUrl;

                await cnx.SaveChangesAsync();
                resp.PhotoPath = imageUrl;
                resp.CourseId = course.Id;

                await _searchSvc.ReindexByCourse(course.Id);
                
            }

            return resp;
        }

        public async Task<UploadCoursePhotoResponse> UploadCourseBannerPhoto(string PhotoBase, int CourseId)
        {
            var resp = new UploadCoursePhotoResponse();
            using (var cnx = new DataContext())
            {

                var course = await cnx.courses.FirstOrDefaultAsync(t => t.Id == CourseId);
                course.CheckExist("Course");

                string generateName = Statics.LetsHash.RandomHash($"{DateTime.Now}");
                await Task.Run(() => SaveImage(PhotoBase, generateName));
                var imageUrl = $"images/{generateName}.jpg";

                course.BannerPhotoUrl = imageUrl;

                await cnx.SaveChangesAsync();
                resp.PhotoPath = imageUrl;
                resp.CourseId = course.Id;

                await _searchSvc.ReindexByCourse(course.Id);

            }

            return resp;
        }


        private void RemoveFile(string Filename, FileType fileType)
        {
            string path = "";

            switch (fileType)
            {
                case FileType.Attachment:
                    path = $"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/";
                    break;
                case FileType.Image:
                    path = $"{ConfigData.Config.EnviromentPath}/wwwroot/images/";
                    break;
                case FileType.Video:
                    path = $"{ConfigData.Config.EnviromentPath}/wwwroot/uploads/";
                    break;
                case FileType.Document:
                    path = $"{ConfigData.Config.EnviromentPath}wwwroot/document/";
                    break;
                default:
                    path = $"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/";
                    break;
            }

            if (File.Exists($"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/{Filename}"))
            {
                File.Delete($"{ConfigData.Config.EnviromentPath}/wwwroot/attachments/{Filename}");
            }

            if (fileType == FileType.Document) {
                if (File.Exists($"{path}{Filename}"))
                {
                    File.Delete($"{path}{Filename}");
                }
            }
        }


    }
}
