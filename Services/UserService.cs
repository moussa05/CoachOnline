using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Student;
using CoachOnline.Statics;
using ITSAuth.Interfaces;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class UserService : IUser
    {
        private readonly ILogger<UserService> _logger;
        private readonly ISubscription _subscription;
        private readonly IEmailApiService _emailSvc;

        public UserService(IServiceProvider svcProvider, ILogger<UserService> logger, IEmailApiService emailSvc)
        {
            _logger = logger;
            _subscription = new SubscriptionService(svcProvider, this);
            _emailSvc = emailSvc;
        }


        public async Task DeleteAccount(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == userId)
                     .Include(c => c.companyInfo)
                     .Include(c => c.AccountCategories)
                     .Include(c => c.OwnedCourses)
                     .ThenInclude(e => e.Episodes)
                     .ThenInclude(a => a.Attachments).FirstOrDefaultAsync();

                user.CheckExist("User");

                if (user.StripeAccountId != null)
                {
                    await DeleteStripeConnectedAccount(user.Id);
                }

                if (user.StripeCustomerId != null)
                {
                    await DeleteStripeCustomerAccount(user.Id);
                }


                await RemoveBillingPlans(user.Id);

                if (user.OwnedCourses != null)
                {
                    foreach (var c in user.OwnedCourses)
                    {
                        if (c.Episodes != null)
                        {
                            foreach (var ep in c.Episodes)
                            {
                                if (ep.Attachments != null)
                                {
                                    foreach (var a in ep.Attachments)
                                    {
                                        Helpers.Extensions.RemoveFile(a.Hash, FileType.Attachment);

                                    }
                                }

                                Helpers.Extensions.RemoveFile(ep.MediaId, FileType.Video);

                                ctx.Episodes.Remove(ep);
                            }
                        }
                        if (c.PhotoUrl != null)
                        {
                            Helpers.Extensions.RemoveFile(c.PhotoUrl, FileType.Image);
                        }



                        ctx.courses.Remove(c);

                    }
                }

                user.EmailAddress = $"_deleted_{user.EmailAddress}_";
                user.SocialAccountId = user.SocialAccountId != null ? $"deleted_{user.SocialAccountId}":null;
                user.companyInfo = null;
                user.Status = UserAccountStatus.DELETED;

                await ctx.SaveChangesAsync();

                await DeleteLoginTokens(user.Id);


            }
        }

        public async Task SendEndOfDiscoveryModeEmails()
        {
            string emailBody = "";

            if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/FreeSubEnds.html"))
            {
                emailBody = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/FreeSubEnds.html");
                emailBody = emailBody.Replace("###COACHESURL###", $"{Statics.ConfigData.Config.WebUrl}");

            }
            else
            {
                return;
            }
            using (var ctx = new DataContext())
            {
                var startTime = new DateTime(2022, 6, 1);
                var usersNotActiveSub = await ctx.users.Where(x => x.UserRole == UserRoleType.STUDENT && (x.EndOdDiscoveryModeStatus == EndOfDiscoveryMode.NOT_SEND || x.EndOdDiscoveryModeStatus == EndOfDiscoveryMode.FIRST_EMAIL) 
                && x.AccountCreationDate.HasValue && x.AccountCreationDate.Value>= startTime && !x.SubscriptionActive).Include(x => x.UserBillingPlans).ToListAsync();


                foreach (var u in usersNotActiveSub)
                {
                    if(u.UserBillingPlans.Any(x=>x.Status== BillingPlanStatus.ACTIVE 
                    || x.Status == BillingPlanStatus.CANCELLED 
                    || x.Status == BillingPlanStatus.AWAITING_PAYMENT
                    || x.Status == BillingPlanStatus.AWAITING_ACTIVATION
                    || x.Status == BillingPlanStatus.PAYMENT_REJECTED))
                    {
                       //do not send email
                    }
                    else
                    {
                        if(u.AccountCreationDate.Value.AddDays(7)<= DateTime.Now)
                        {
                            if(u.EndOdDiscoveryModeStatus == EndOfDiscoveryMode.NOT_SEND)
                            {
                                u.EndOdDiscoveryModeEmailSendDate = DateTime.Now;
                                u.EndOdDiscoveryModeStatus = EndOfDiscoveryMode.FIRST_EMAIL;

                                await ctx.SaveChangesAsync();

                                await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage()
                                {
                                    AuthorEmail = "info@coachs-online.com",
                                    AuthorName = "Coachs-online",
                                    ReceiverEmail = u.EmailAddress,
                                    Topic = "Nouvel abonnement disponible",
                                    Body = emailBody,
                                    ReceiverName = $"{u.FirstName?.ToString()} {u.Surname?.ToString()}"
                                });
                            }
                            else if(u.EndOdDiscoveryModeStatus == EndOfDiscoveryMode.FIRST_EMAIL 
                                && u.EndOdDiscoveryModeEmailSendDate.HasValue
                                && u.EndOdDiscoveryModeEmailSendDate.Value.AddDays(14)<DateTime.Now)
                            {
                                u.EndOdDiscoveryModeStatus = EndOfDiscoveryMode.SECOND_EMAIL;
                                await ctx.SaveChangesAsync();

                                await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage()
                                {
                                    AuthorEmail = "info@coachs-online.com",
                                    AuthorName = "Coachs-online",
                                    ReceiverEmail = u.EmailAddress,
                                    Topic = "Nouvel abonnement disponible",
                                    Body = emailBody,
                                    ReceiverName = $"{u.FirstName?.ToString()} {u.Surname?.ToString()}"
                                });
                            }
                           

                  
                        }
                    }
                }
            }
        }
            public async Task<GetAuthTokenResponse> RegisterSocialLogin(string socialId, string provider, string email, string firstName, string lastName, string pictrueUrl, UserRoleType role, string deviceInfo, string placeInfo, string ipAddress, string gender, int? yearOfBirth, int? professionId, int? libraryId, string affiliateLink)
        {
            using (var ctx = new DataContext())
            {
                email = email.ToLower().Trim();
                var alreadyExists = await ctx.users.AnyAsync(t => t.EmailAddress.ToLower().Trim() == email && t.Status != UserAccountStatus.DELETED);
                if (alreadyExists)
                {
                    throw new CoachOnlineException("User with given email already exists", CoachOnlineExceptionState.AlreadyExist);
                }
                var alreadyExists2 = await ctx.users.AnyAsync(t => t.SocialAccountId == socialId && t.SocialProvider == provider && t.Status != UserAccountStatus.DELETED);
                if (alreadyExists2)
                {
                    throw new CoachOnlineException("User already exists", CoachOnlineExceptionState.AlreadyExist);
                }

                if (!(role == UserRoleType.COACH || role == UserRoleType.STUDENT || role == UserRoleType.INSTITUTION_STUDENT))
                {
                    throw new CoachOnlineException("User account role does not exists", CoachOnlineExceptionState.DataNotValid);
                }

                if(role == UserRoleType.INSTITUTION_STUDENT)
                {
                    if(string.IsNullOrEmpty(gender) || !yearOfBirth.HasValue || !libraryId.HasValue || !professionId.HasValue)
                    {
                        throw new CoachOnlineException("To register institution student you need to provide library id, gender, profession and year of birth of a student.", CoachOnlineExceptionState.DataNotValid);
                    }

                    var lib = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryId.Value && t.AccountStatus != AccountStatus.DELETED);
                    lib.CheckExist("Library");
                    var profession = await ctx.Professions.FirstOrDefaultAsync(t => t.Id == professionId.Value);
                    profession.CheckExist("Profession");
                }

                string picUrl = null;
                if(!string.IsNullOrEmpty(pictrueUrl))
                {
                    picUrl = Extensions.SaveImageFormUrl(pictrueUrl, LetsHash.RandomHash(email));
                }

                var lastTerms = await ctx.Terms.OrderByDescending(x => x.Created).FirstOrDefaultAsync();
                if (lastTerms == null)
                {
                    lastTerms = new Terms();
                }


                var user = new User() { EmailAddress = email, SocialProvider = provider, SocialLogin = true, SocialAccountId = socialId, 
                    AvatarUrl = picUrl, AccountCreationDate = DateTime.Now, FirstName = firstName, Surname = lastName, UserRole = role , Status = UserAccountStatus.CONFIRMED, TermsAccepted = lastTerms, AffiliatorType = AffiliateModelType.Regular};

                if (role == UserRoleType.INSTITUTION_STUDENT)
                {
                    user.InstitutionId = libraryId;
                    user.ProfessionId = professionId;
                    user.Gender = gender;
                    user.YearOfBirth = yearOfBirth;
                }
                int? affiliateUserId = null;
                AffiliateModelType modelType = AffiliateModelType.Regular;
                if ((role == UserRoleType.STUDENT || role == UserRoleType.COACH) && !string.IsNullOrEmpty(affiliateLink))
                {
                        var affiliate = await ctx.AffiliateLinks.Where(t => t.GeneratedToken == affiliateLink).FirstOrDefaultAsync();

                        if (affiliate == null)
                        {
                            throw new CoachOnlineException("Cannot register with from this affiliate link. It does not exist.", CoachOnlineExceptionState.NotExist);
                        }

                        affiliateUserId = affiliate.UserId;

                  
                    if (affiliate.CouponCode != null && user.UserRole == UserRoleType.STUDENT)
                    {
                        var coupon = await ctx.PromoCoupons.FirstOrDefaultAsync(x => x.Id == affiliate.CouponCode);

                        if (coupon != null)
                        {
                            user.CouponId = coupon.Id;
                            user.CouponValidDate = DateTime.Now.AddMonths(1).AddDays(-1);
                        }

                        user.AllowHiddenProducts = affiliate.WithTrialPlans;
                    }

                    var userType = await ctx.users.FirstOrDefaultAsync(x=>x.Id == affiliateUserId.Value);
                    if(userType != null && !affiliate.ForCoach)
                    {
                        modelType = userType.AffiliatorType;
                    }
                }

                ctx.users.Add(user);

                await ctx.SaveChangesAsync();

                await GenerateNick(user.Id);

                if (affiliateUserId.HasValue)
                {
                    var newAffiliate = new Affiliate();
                    newAffiliate.CreationDate = DateTime.Now;
                    newAffiliate.HostUserId = affiliateUserId.Value;
                    newAffiliate.AffiliateUserId = user.Id;
                    newAffiliate.IsAffiliateACoach = role == UserRoleType.COACH;
                    newAffiliate.AffiliateModelType = modelType;
                    ctx.Affiliates.Add(newAffiliate);

                    await ctx.SaveChangesAsync();
                }

                return await SocialLogin(socialId, provider, deviceInfo, placeInfo, ipAddress);
            }
        }

        public async Task<GetAuthTokenResponse> SocialLogin(string socialId, string provider,string deviceInfo,string placeInfo, string ipAddress)
        {
            using (var ctx = new DataContext())
            {
                GetAuthTokenResponse response = new GetAuthTokenResponse();
                response.UserInfo = new UserAuthInfo();
                var user = await ctx.users.FirstOrDefaultAsync(t => t.SocialLogin.HasValue && t.SocialLogin.Value && t.SocialProvider == provider && t.SocialAccountId == socialId && t.Status != UserAccountStatus.DELETED);
                user.CheckExist("User");

                if (user.Status == UserAccountStatus.AWAITING_EMAIL_CONFIRMATION)
                {
                    throw new CoachOnlineException("Account not confirmed", CoachOnlineExceptionState.PermissionDenied);
                }
                if (user.Status == UserAccountStatus.BANNED)
                {
                    throw new CoachOnlineException("Account is banned", CoachOnlineExceptionState.PermissionDenied);
                }

                var authToken = LetsHash.RandomHash(user.EmailAddress);
                response.AuthToken = authToken;
                response.UserInfo.Email = user.EmailAddress ?? "";
                response.UserInfo.Name = user.FirstName ?? "";
                response.UserInfo.UserRole = user.UserRole.ToString();
                response.UserInfo.SubscriptionActive = user.SubscriptionActive;



                if (user.WithdrawalsEnabled)
                {
                    response.UserInfo.StripeVerificationStatus = 3;
                }
                else if (user.PaymentsEnabled)
                {
                    response.UserInfo.StripeVerificationStatus = 2;

                }
                else if (!string.IsNullOrEmpty(user.StripeAccountId))
                {
                    response.UserInfo.StripeVerificationStatus = 1;

                }
                else
                {
                    response.UserInfo.StripeVerificationStatus = 0;

                }



                response.UserInfo.StripeCustomerId = user.StripeCustomerId;
                if (string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    await _subscription.CreateUserStripeCustomerAccount(user);

                    response.UserInfo.StripeCustomerId = (await GetUserById(user.Id)).StripeCustomerId;
                }




                if (user.UserLogins == null)
                {
                    user.UserLogins = new List<UserLogins>();
                }
                user.UserLogins.Add(new UserLogins
                {
                    AuthToken = authToken,
                    Created = ConvertTime.ToUnixTimestampLong(DateTime.Now),
                    DeviceInfo = deviceInfo,
                    IpAddress = ipAddress,
                    PlaceInfo = placeInfo,
                    Disposed = false,
                    ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30))
                });
                await ctx.SaveChangesAsync();




                await Authenticate(authToken);

                return response;
            }

            
        
        }

        private async Task DeleteLoginTokens(int userId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == userId).Include(t => t.UserLogins).FirstOrDefaultAsync();
                user.CheckExist("User");
                if (user.UserLogins != null)
                {
                    ctx.userLogins.RemoveRange(user.UserLogins);

                    await ctx.SaveChangesAsync();
                }
            }
        }
        private async Task RemoveBillingPlans(int userId)
        {
            using (var ctx = new DataContext())
            {
                var billingPlans = await ctx.UserBillingPlans.Where(t => t.UserId == userId).Include(x => x.StudentCardData).ToListAsync();

                foreach (var b in billingPlans)
                {
                    if (b.StudentCardData != null)
                    {
                        foreach (var s in b.StudentCardData)
                        {
                            if (s.StudentsCardPhotoName != null)
                            {
                                Helpers.Extensions.RemoveFile(s.StudentsCardPhotoName, FileType.StudentCard);
                            }
                        }
                    }

                    ctx.UserBillingPlans.Remove(b);
                }

                await ctx.SaveChangesAsync();
            }
        }

        private async Task DeleteStripeCustomerAccount(int userId)
        {
            try
            {
                using (var ctx = new DataContext())
                {
                    var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                    user.CheckExist("User");
                    if (!string.IsNullOrEmpty(user.StripeCustomerId))
                    {
                        CustomerService cs = new CustomerService();
                        var customer = await cs.ListAsync(new CustomerListOptions() { Email = user.EmailAddress });
                        if (customer != null && customer.Count() > 0)
                        {
                            await cs.DeleteAsync(user.StripeCustomerId);
                        }
                    }

                    user.StripeCustomerId = null;

                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new CoachOnlineException(ex.ToString(), CoachOnlineExceptionState.UNKNOWN);
            }
        }

        private async Task DeleteStripeConnectedAccount(int userId)
        {
            try
            {
                using (var ctx = new DataContext())
                {
                    var user = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId);
                    user.CheckExist("User");
                    if (!string.IsNullOrEmpty(user.StripeAccountId))
                    {
                        AccountService acSvc = new AccountService();
                        await acSvc.DeleteAsync(user.StripeAccountId);
                    }

                    user.StripeAccountId = null;
                    user.PaymentsEnabled = false;
                    user.WithdrawalsEnabled = false;

                    await ctx.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new CoachOnlineException(ex.ToString(), CoachOnlineExceptionState.UNKNOWN);
            }
        }

        public async Task<AccountLink> UpdateUserStripeConnectedAccountLink(int userId)
        {
            var user = await GetUserById(userId);

            if(string.IsNullOrEmpty(user.StripeAccountId))
            {
                throw new CoachOnlineException("User does not have stripe connected account created.", CoachOnlineExceptionState.NotExist);
            }

            var options = new AccountLinkCreateOptions
            {
                Account = user.StripeAccountId,
                RefreshUrl = $"{ConfigData.Config.WebUrl}/billing",
                ReturnUrl = $"{ConfigData.Config.WebUrl}/billing",
                Type = "account_onboarding",
                //Type = "account_update"
            };
            var service = new AccountLinkService();
            var response = await service.CreateAsync(options);

            return response;
        }


        private DateTime GetFirstLogInDate(int user)
        {
            using (var ctx = new DataContext())
            {
                var u = ctx.users.Where(t => t.Id == user).Include(l => l.UserLogins).FirstOrDefault();

                if (u.AccountCreationDate.HasValue)
                {
                    return u.AccountCreationDate.Value;
                }
                if (u.UserLogins.Any())
                {
                    var firstLogin = u.UserLogins.OrderBy(t => t.Created).FirstOrDefault();

                    return ConvertTime.FromUnixTimestamp(firstLogin.Created);
                }

                return DateTime.Now;
            }
        }

        public async Task<EndUserProfileDataResponse> GetUserProfileData(User user)
        {

            EndUserProfileDataResponse response = new EndUserProfileDataResponse();

            response.UserId = user.Id;
            response.Nick = user.Nick;
            response.Email = user.EmailAddress;
            response.Bio = user.Bio ?? "";
            response.City = user.City ?? "";
            response.FirstName = user.FirstName ?? "";
            response.PhotoUrl = user.AvatarUrl ?? "";
            response.LastName = user.Surname ?? "";
            response.YearOfBirth =  user.YearOfBirth;
            response.Country = user.Country ?? "";
            response.PostalCode = user.PostalCode ?? "";
            response.Address = user.Adress ?? "";
            response.StripeCustomerId = user.StripeCustomerId ?? "";
            response.Subscription = new SubscriptionResponse();
            response.Subscription.Card = new CardResponse();
            response.Gender = user.Gender;
            response.PhoneNo = user.PhoneNo;
            response.SocialLogin = user.SocialLogin.HasValue && user.SocialLogin.Value;
            response.EmailConfirmed = user.EmailConfirmed;
            response.RegistrationDate = user.AccountCreationDate.HasValue ? user.AccountCreationDate.Value : GetFirstLogInDate(user.Id);
            response.TrialEndDate = response.RegistrationDate.HasValue? response.RegistrationDate.Value.AddDays(3) : new DateTime(2021,10,1).AddDays(3);
            response.TrialActive = response.TrialEndDate >= DateTime.Now;
            response.AffiliatorType = user.AffiliatorType;

            UserBillingPlan subscription = null;
            if (!string.IsNullOrEmpty(user.StripeCustomerId))
            {

                subscription = await _subscription.GetUserCurrentSubscriptionPlan(user.Id);

                var customer = await _subscription.GetStripeCustomer(user.StripeCustomerId);
                var paymentMethod = await _subscription.GetCustomerStripePaymentMethod(customer.InvoiceSettings.DefaultPaymentMethodId);
                if (subscription != null)
                {
                    if (subscription.StripeSubscriptionId != null)
                    {
                        var stripeSubscription = await _subscription.GetCustomerStripeSubscription(subscription.StripeSubscriptionId);
                        if (stripeSubscription != null)
                        {
                            response.Subscription.SelectedPlanId = subscription.Id;
                            response.Subscription.NextBillingTime = !stripeSubscription.CancelAtPeriodEnd ? (DateTime?)stripeSubscription.CurrentPeriodEnd : null;
                            response.Subscription.SubscriptionId = stripeSubscription.Id;
                            response.Subscription.SubscriptionName = subscription.BillingPlanType.Name;
                            response.Subscription.SubscriptionCancelAt = subscription.ExpiryDate;
                            if (stripeSubscription.Items.Data.Count > 0)
                            {
                                var item = stripeSubscription.Items.Data[0];
                                if (item.Plan.IntervalCount > 1)
                                {
                                    response.Subscription.Period = $"{item.Plan.IntervalCount} {item.Plan.Interval}s";
                                }
                                else
                                {
                                    response.Subscription.Period = item.Plan.Interval;
                                }
                                response.Subscription.Currency = item.Plan.Currency;

                                if (item.Plan.AmountDecimal.HasValue)
                                    response.Subscription.Price = item.Plan.AmountDecimal.Value / 100;
                                else response.Subscription.Price = 0;
                            }

                        }
                    }
                    else
                    {
                        response.Subscription.SelectedPlanId = subscription.Id;
                        response.Subscription.SubscriptionName = subscription.BillingPlanType.Name;


                        if (subscription.BillingPlanType.Price.Period.HasValue && subscription.BillingPlanType.Price.Period.Value > 1)
                        {
                            response.Subscription.Period = $"{subscription.BillingPlanType.Price.Period.Value} {subscription.BillingPlanType.Price.PeriodType}s";
                        }
                        else
                        {
                            response.Subscription.Period = subscription.BillingPlanType.Price.PeriodType;
                        }
                        response.Subscription.Currency = subscription.BillingPlanType.Currency;
                        response.Subscription.Price = subscription.BillingPlanType.Price.Amount.Value;
                        response.Subscription.NextBillingTime = subscription.PlannedActivationDate;
                    }


                }
                if (paymentMethod != null)
                {
                    response.Subscription.PaymentMethodId = paymentMethod.Id;
                    if (paymentMethod.Card != null)
                    {
                        response.Subscription.Card.Brand = paymentMethod.Card.Brand;
                        response.Subscription.Card.Last4Digits = paymentMethod.Card.Last4;
                        response.Subscription.Card.ValidTo = $"{paymentMethod.Card.ExpMonth}/{paymentMethod.Card.ExpYear}";
                        response.Subscription.Card.Country = paymentMethod.Card.Country;
                    }
                }


            }



            response.AccessType = await GetUserAccessType(user.Id, response.TrialEndDate);


            return response;
        }

        public async Task<AccessType> GetUserAccessType(int userId, DateTime userTrialEnd)
        {
            var result = AccessType.NO_ACCESS;
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.FirstOrDefaultAsync(t => t.Id == userId && t.Status != UserAccountStatus.DELETED);
                if (usr != null)
                {
                    if(userTrialEnd>= DateTime.Now)
                    {
                        result = AccessType.PROMO;
                    }

                    if (usr.UserRole == UserRoleType.COACH)
                    {
                        var anyApproved = await ctx.courses.AnyAsync(x => x.UserId == usr.Id && x.State == CourseState.APPROVED);
                        if(anyApproved)
                        {
                            result = AccessType.FULL;
                            return result;
                        }
                    }
                    else if (usr.UserRole == UserRoleType.INSTITUTION_STUDENT && usr.InstitutionId.HasValue)
                    {
                        var library = await ctx.LibraryAccounts.Where(t => t.Id == usr.InstitutionId.Value).Include(s=>s.Subscriptions).FirstOrDefaultAsync();
                        var anyActive = library.Subscriptions.Any(t => t.Status == LibrarySubscriptionStatus.ACTIVE && t.SubscriptionStart.Date<= DateTime.Today && t.SubscriptionEnd.Date >= DateTime.Today);
                        if(anyActive)
                        {
                            result = AccessType.FULL;
                            return result;
                        }
                    }
                    else if (usr.UserRole == UserRoleType.STUDENT)
                    {
                        UserBillingPlan sub = await _subscription.GetUserActiveSubscriptionPlan(usr.Id);
                        if (sub == null)
                        {
                            return result;
                        }
                        if (sub.Status == BillingPlanStatus.ACTIVE)
                        {
                            result = AccessType.FULL;
                            return result;
                        }
                    }
                }
            }

            return result;
        }

        public async Task UpdateBasicUserData(User u, string Name, string Surname, int? YearOfBirth, string City, string Bio, string Country, string PostalCode, string Address, string gender, string phoneNo, string nick)
        {
            using (var cnx = new DataContext())
            {
                var user = await cnx.users.FirstOrDefaultAsync(x => x.Id == u.Id);

                if(!string.IsNullOrEmpty(nick))
                {
                    if(user.Nick != nick)
                    {
                        var isTaken = await cnx.users.Select(x => x.Nick).AnyAsync(t => t.ToLower() == nick.ToLower());

                        if(isTaken)
                        {
                            throw new CoachOnlineException("Such nick already exists", CoachOnlineExceptionState.AlreadyExist);
                        }
                        else
                        {
                            user.Nick = nick;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(City))
                {
                    user.City = City;
                }

                if (!string.IsNullOrEmpty(Name))
                {
                    user.FirstName = Name;
                }

                if (!string.IsNullOrEmpty(Surname))
                {
                    user.Surname = Surname;
                }


                user.Bio = Bio;


                if (!string.IsNullOrEmpty(Country))
                {
                    user.Country = Country;
                }

                if (!string.IsNullOrEmpty(PostalCode))
                {
                    user.PostalCode = PostalCode;
                }

                if (!string.IsNullOrEmpty(Address))
                {
                    user.Adress = Address;
                }


                if (YearOfBirth.HasValue)
                {
                    if (YearOfBirth.Value < 1900 || YearOfBirth.Value > DateTime.Today.Year - 1)
                    {
                        throw new CoachOnlineException("Wrong year of birth", CoachOnlineExceptionState.DataNotValid);
                    }

                    user.YearOfBirth = YearOfBirth.Value;
                }
                else
                {
                    user.YearOfBirth = null;
                }

                if(!string.IsNullOrEmpty(gender))
                {
                    user.Gender = gender;
                }

                if (!string.IsNullOrEmpty(phoneNo))
                {
                    user.PhoneNo = phoneNo;
                }


                await cnx.SaveChangesAsync();

                if (!string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var stripeCustomer = await _subscription.GetStripeCustomer(user.StripeCustomerId);
                    if (stripeCustomer != null)
                    {
                        CustomerUpdateOptions opts = new CustomerUpdateOptions();
                        opts.Address = new AddressOptions()
                        {
                            City = City,
                            Country = Country,
                            Line1 = Address,
                            PostalCode = PostalCode
                        };
                        if (!string.IsNullOrEmpty(phoneNo))
                        {
                            opts.Phone = phoneNo;
                        }
                        opts.Name = $"{Name} {Surname}";
                        opts.Description = $"{Name} {Surname}";

                        if (opts.Metadata == null)
                        {
                            opts.Metadata = new Dictionary<string, string>();
                            opts.Metadata.Add("UserId", user.Id.ToString());
                        }

                        var custMetadataOther = stripeCustomer.Metadata.Where(t => t.Key != "UserId").ToList();

                        foreach (var v in custMetadataOther)
                        {
                            opts.Metadata.Add(v.Key, v.Value);
                        }



                        CustomerService cSvc = new CustomerService();
                        var customer = await cSvc.UpdateAsync(user.StripeCustomerId, opts);
                    }
                }

            }
        }

        public async Task<User> GetUserByTokenAllowNullAsync(string token)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    return null;

                }
                var interestingLogin = user.UserLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                if (interestingLogin == null)
                {
                    return null;
                }
                if (interestingLogin.Disposed)
                {
                    return null;
                }
                if (interestingLogin.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    return null;

                }
                u = user;
            }

            return u.WithoutPassword();
        }

        public async Task<User> GetUserByTokenAsync(string token)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    var admin = await cnx.Admins.Where(x => x.AdminLogins.Any(p => p.AuthToken == token))
                   .FirstOrDefaultAsync();
                    //.Include(x => x.AdminLogins)
                    //var admin = await cnx.twoFATokens.Where(x => x.)


                    throw new CoachOnlineException("Auth Token never existed. Or not match with any user.", CoachOnlineExceptionState.NotExist);


                }
                var interestingLogin = user.UserLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                if (interestingLogin == null)
                {
                    throw new CoachOnlineException("AuthToken never existed.", CoachOnlineExceptionState.NotExist);
                }
                if (interestingLogin.Disposed)
                {
                    throw new CoachOnlineException("AuthToken is Disposed", CoachOnlineExceptionState.Expired);
                }
                if (interestingLogin.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    interestingLogin.Disposed = true;
                    await cnx.SaveChangesAsync();
                    throw new CoachOnlineException("AuthToken is Outdated", CoachOnlineExceptionState.Expired);

                }
                u = user;
            }

            return u.WithoutPassword();
        }


        public async Task<Model.User> GetUserByTokenAsync(string token, int CourseId, int EpisodeId)
        {
            Model.User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    var admin = await cnx.Admins.Where(x => x.AdminLogins.Any(p => p.AuthToken == token))
                   .FirstOrDefaultAsync();
                    //.Include(x => x.AdminLogins)
                    //var admin = await cnx.twoFATokens.Where(x => x.)
                    if (admin == null)
                    {

                        throw new CoachOnlineException("Auth Token never existed. Couldn't login as admin.,", CoachOnlineExceptionState.NotExist);

                    }
                    else
                    {
                        _logger.LogWarning("Admin using object like user!");
                        var adminAsUser = cnx.users.Where(x => x.OwnedCourses.Any(p => p.Id == CourseId && p.Episodes.Any(o => o.Id == EpisodeId))).FirstOrDefault();
                        if (adminAsUser == null)
                        {
                            throw new CoachOnlineException($"User with course {CourseId} & episode {EpisodeId} not exist. Skiiping.", CoachOnlineExceptionState.NotExist);

                        }
                        else
                        {

                            return adminAsUser.WithoutPassword();
                        }
                    }
                }
                var interestingLogin = user.UserLogins
                    .Where(x => x.AuthToken == token)
                    .FirstOrDefault();
                if (interestingLogin == null)
                {
                    throw new CoachOnlineException("AuthToken never existed.", CoachOnlineExceptionState.NotExist);
                }
                if (interestingLogin.Disposed)
                {
                    throw new CoachOnlineException("AuthToken is Disposed", CoachOnlineExceptionState.Expired);
                }
                if (interestingLogin.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    interestingLogin.Disposed = true;
                    await cnx.SaveChangesAsync();
                    throw new CoachOnlineException("AuthToken is Outdated", CoachOnlineExceptionState.Expired);

                }
                u = user;
            }

            return u.WithoutPassword();
        }


        public async Task<User> GetUserById(int userId)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.FirstOrDefaultAsync(u => u.Id == userId && u.Status != UserAccountStatus.DELETED);
                if (user == null)
                {
                    throw new CoachOnlineException("User does not exist", CoachOnlineExceptionState.NotExist);
                }
                u = user;
            }

            return u.WithoutPassword();
        }

        public async Task<User> GetUserByIdAllowNull(int userId)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users.FirstOrDefaultAsync(u => u.Id == userId && u.Status != UserAccountStatus.DELETED);
                if (user == null)
                {
                    return null;
                }
                u = user;
            }

            return u.WithoutPassword();
        }

        public async Task<bool> IsUserOwnerOfCourse(int userId, int courseId)
        {
            
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.Where(t => t.Id == userId).Include(c => c.OwnedCourses).FirstOrDefaultAsync();

                if (usr != null && usr.OwnedCourses != null)
                {
                    return usr.OwnedCourses.Any(x => x.Id == courseId);
                }

                return false;
            }
        }

        public async Task<bool> IsUserOwnerOfEpisode(int userId, int episodeId)
        {
            using (var ctx = new DataContext())
            {
                var usr = await ctx.users.Where(t => t.Id == userId).Include(c => c.OwnedCourses).ThenInclude(ep => ep.Episodes).FirstOrDefaultAsync();

                if (usr != null && usr.OwnedCourses != null)
                {
                    bool anyValid = false;

                    foreach (var c in usr.OwnedCourses)
                    {
                        bool result = c.Episodes.Any(x => x.Id == episodeId);
                        if (result)
                        {
                            anyValid = true;
                            break;
                        }
                    }

                    return anyValid;
                }

                return false;
            }
        }

        public async Task<User> GetAdminByTokenAsync(string token)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.Admins
                    .Include(x => x.AdminLogins)
                    .Where(x => x.AdminLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
                    throw new CoachOnlineException("Auth Token never existed. Or not match with any user.", CoachOnlineExceptionState.NotExist);
                }

                u = new User() { EmailAddress = user.Email, UserRole = UserRoleType.ADMIN, Id = user.Id };
            }

            return u.WithoutPassword();
        }



        public async Task<User> Authenticate(string token)
        {
            User u = null;
            try
            {
                u = await GetUserByTokenAsync(token);

            }
            catch (Exception)
            {
                u = null;
            }
            if (u == null)
            {
                u = await GetAdminByTokenAsync(token);
            }


            return u;
        }

        public async Task<User> Authenticate(string email, string secret)
        {
            User user = null;
            string secretHashed = LetsHash.ToSHA512(secret);
            using (var ctx = new DataContext())
            {
                var admin = await ctx.Admins.Where(t => t.Email.ToLower() == email.ToLower()).Select(x => new User()
                {
                    Password = x.Password,
                    EmailAddress = x.Email,
                    UserRole = UserRoleType.ADMIN,
                    Id = x.Id
                }).FirstOrDefaultAsync();

                if (admin != null)
                {
                    user = admin;
                }
                else
                {
                    user = await ctx.users.FirstOrDefaultAsync(t => t.EmailAddress.ToLower() == email.ToLower());
                }

                user.CheckExist("User");
                CompareHashes(secretHashed, user.Password, "Password");
            }



            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so return user details without password
            return user.WithoutPassword();
        }


        #region private members 

        private void CompareHashes(string first, string second, string HashFieldName)
        {
            if (first != second)
            {
                throw new CoachOnlineException($"Wrong {HashFieldName} provided.", CoachOnlineExceptionState.WrongPassword);
            }
        }

        public async Task UpdateUserEmail(int userId, string email)
        {
            email = email.Trim().ToLower();
            using (var ctx = new DataContext())
            {
                var emailInUse = await ctx.users.AnyAsync(t => t.EmailAddress.Trim().ToLower() == email);

                if(emailInUse)
                {
                    throw new CoachOnlineException($"Email {email} is already in use.", CoachOnlineExceptionState.AlreadyExist);
                }
                var user = await ctx.users.Where(u => u.Id == userId && u.Status == UserAccountStatus.CONFIRMED).Include(f => f.TwoFATokens).FirstOrDefaultAsync();
                user.CheckExist("User");

                if (email==null || !new EmailAddressAttribute().IsValid(email))
                {
                    throw new CoachOnlineException($"{email} is not a valid email address", CoachOnlineExceptionState.DataNotValid);
                }

                if (user.TwoFATokens == null)
                {
                    user.TwoFATokens = new List<TwoFATokens>();
                }
                string token = LetsHash.RandomHash(user.EmailAddress+ DateTime.Now.ToString());
                DateTime validateMaxDate = DateTime.Now.AddDays(7);
                var unix = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddHours(2));
                user.TwoFATokens.Add(new TwoFATokens() { AdditionalInfo = email, Deactivated = false, Type = TwoFaTokensTypes.EMAIL_CHANGE_CONFIRMATION, ValidateTo = unix, Token = token });


                await ctx.SaveChangesAsync();


                //prepare email message
                string body = $"<a href='{Statics.ConfigData.Config.SiteUrl}/api/Profile/ConfirmUserEmail?Token={token}'>confirm account </a> <br><br> Token: {token}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/ChangeEmailAddress.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/ChangeEmailAddress.html");
                    body = body.Replace("###CONFIRMATIONURL###", $"{Statics.ConfigData.Config.SiteUrl}/api/Profile/ConfirmUserEmail?Token={token}");
                    body = body.Replace("###NEW_EMAIL###", email);

                    Console.WriteLine("body changed");
                }

                await _emailSvc.SendEmailAsync(new ITSAuth.Model.EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = user.EmailAddress,
                    ReceiverName = $"{user.FirstName} {user.Surname}",
                    Topic = "Coachs-online confirme votre adresse e-mail"
                });

                Console.WriteLine($"email sent to address {user.EmailAddress}");


            }
        }

        public async Task<UserRoleType> ConfirmEmailUpdate(string confirmToken)
        {
            using(var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t=>t.Status != UserAccountStatus.DELETED).Include(fa => fa.TwoFATokens).Where(t => t.TwoFATokens.Any(t => t.Token == confirmToken && t.Type == TwoFaTokensTypes.EMAIL_CHANGE_CONFIRMATION)).FirstOrDefaultAsync();
                user.CheckExist("User");

                var userToken = user.TwoFATokens.FirstOrDefault(t => t.Token == confirmToken && t.Type == TwoFaTokensTypes.EMAIL_CHANGE_CONFIRMATION);
                userToken.CheckExist("Token");

                if (userToken.Deactivated)
                {
                    throw new CoachOnlineException("Token not valid.", CoachOnlineExceptionState.DataNotValid);
                }

                if (ConvertTime.ToUnixTimestampLong(DateTime.Now) >= userToken.ValidateTo)
                {
                    throw new CoachOnlineException($"Token expired at {ConvertTime.FromUnixTimestamp(userToken.ValidateTo)}.", CoachOnlineExceptionState.Expired);
                }

                if (userToken.AdditionalInfo == null || !new EmailAddressAttribute().IsValid(userToken.AdditionalInfo))
                {
                    throw new CoachOnlineException($"{userToken.AdditionalInfo} is not a valid email address", CoachOnlineExceptionState.DataNotValid);
                }

                var emailInUse = await ctx.users.AnyAsync(t => t.EmailAddress.Trim().ToLower() == userToken.AdditionalInfo);

                if (emailInUse)
                {
                    throw new CoachOnlineException($"Email {userToken.AdditionalInfo} is already in use.", CoachOnlineExceptionState.AlreadyExist);
                }

                user.EmailAddress = userToken.AdditionalInfo;

                userToken.Deactivated = true;

                user.EmailConfirmed = true;

                await ctx.SaveChangesAsync();

                if (!string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    var stripeCustomer = await _subscription.GetStripeCustomer(user.StripeCustomerId);
                    if (stripeCustomer != null)
                    {
                        CustomerUpdateOptions opts = new CustomerUpdateOptions();

                        opts.Email = user.EmailAddress;

                        if (opts.Metadata == null)
                        {
                            opts.Metadata = new Dictionary<string, string>();
                            opts.Metadata.Add("UserId", user.Id.ToString());
                        }

                        var custMetadataOther = stripeCustomer.Metadata.Where(t => t.Key != "UserId").ToList();

                        foreach (var v in custMetadataOther)
                        {
                            opts.Metadata.Add(v.Key, v.Value);
                        }



                        CustomerService cSvc = new CustomerService();
                        var customer = await cSvc.UpdateAsync(user.StripeCustomerId, opts);
                    }
                }

                return user.UserRole;
            }
        }

        public async Task GenerateNick(int userId)
        {
            using(var ctx = new DataContext())
            {
                var u = await ctx.users.Where(x => x.Id == userId && x.Status != UserAccountStatus.DELETED).FirstOrDefaultAsync();
                u.CheckExist("User");
                if (u.Nick == null)
                {
                    var nick = u.EmailAddress.Split('@').FirstOrDefault();

                    var alreadyExists = await ctx.users.AnyAsync(x => x.Nick.ToLower() == nick.ToLower());

                    if (alreadyExists)
                    {
                        var temp = nick + "1";
                        int id = 1;
                        while (await ctx.users.AnyAsync(x => x.Nick.ToLower() == temp.ToLower()))
                        {
                            temp += id.ToString();
                            id++;
                        }

                        u.Nick = temp;
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        u.Nick = nick;
                        await ctx.SaveChangesAsync();

                    }
                }
            }
            
        }


        #endregion
    }
}
