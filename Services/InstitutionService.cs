using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.B2B;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
using ITSAuth.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class InstitutionService: IInstitution
    {
        private readonly ILogger<InstitutionService> _logger;
        private readonly IEmailApiService _emailSvc;
        private readonly IAuthAsync _authSvc;
        private readonly IUser _userSvc;
        public InstitutionService(ILogger<InstitutionService> logger, IEmailApiService emailSvc, IAuthAsync authSvc, IUser userSvc)
        {
            _logger = logger;
            _emailSvc = emailSvc;
            _authSvc = authSvc;
            _userSvc = userSvc;
        }

        public async Task RegisterWithInstitution(int institutionId, int professionId, string email, string password, string repeat, string gender, int yearOfBirth, string firstName, string lastName, string phoneNo,
            string city, string country, string region)
        {
            using(var ctx = new DataContext())
            {
                if (password != repeat)
                {
                    throw new CoachOnlineException("Passwords don't match.", CoachOnlineExceptionState.PasswordsNotMatch);
                }
                email = email.ToLower().Trim();
                var exists = await ctx.users.AnyAsync(t => t.EmailAddress.ToLower().Trim() == email);

                if(exists)
                {
                    throw new CoachOnlineException("Such user already exists.", CoachOnlineExceptionState.AlreadyExist);
                }

                var institution = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == institutionId && t.AccountStatus == AccountStatus.ACTIVE);
                institution.CheckExist("Library");
                var profession = await ctx.Professions.FirstOrDefaultAsync(t=>t.Id == professionId);
                profession.CheckExist("Profession");

                var lastTerms = await ctx.Terms.OrderBy(x => x.Created).LastOrDefaultAsync();
                if (lastTerms == null)
                {
                    lastTerms = new Terms();
                }
              

                Model.User u = new Model.User();
                u.EmailAddress = email;
                u.InstitutionId = institution.Id;
                u.UserRole = UserRoleType.INSTITUTION_STUDENT;
                u.Password = LetsHash.ToSHA512(password);
                u.Gender = gender;
                u.ProfessionId = profession.Id;
                u.YearOfBirth = yearOfBirth;
                u.Status = UserAccountStatus.CONFIRMED;
                u.SubscriptionActive = false;
                u.TermsAccepted = lastTerms;
                u.AccountCreationDate = DateTime.Now;
                u.FirstName = firstName;
                u.Surname = lastName;
                u.PhoneNo = phoneNo;
                u.EmailConfirmed = false;
                u.AffiliatorType = AffiliateModelType.Regular;
                u.City = city;
                u.Country = country;
                u.Region = region;
                ctx.users.Add(u);

                await ctx.SaveChangesAsync();

                await _userSvc.GenerateNick(u.Id);

                string confirmationToken = await _authSvc.CreateEmailConfirmationToken(email);

                string body = $"<a href='{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}'>confirm account </a> <br><br> Token: {confirmationToken}";
                if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html"))
                {
                    body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/EmailConfirmation.html");
                    body = body.Replace("##CONFIRMATIONURL###", $"{Statics.ConfigData.Config.SiteUrl}/api/Authentication/ConfirmEmailToken?Token={confirmationToken}");

                }

                await _emailSvc.SendEmailAsync(new EmailMessage
                {
                    AuthorEmail = "info@coachs-online.com",
                    AuthorName = "Coachs-online",
                    Body = body,
                    ReceiverEmail = email,
                    ReceiverName = "",
                    Topic = "Coachs-online confirme votre adresse e-mail"
                });


            }
        }

        public async Task<List<Profession>> GetProfessions()
        {
            var data = new List<Profession>();
            using(var ctx = new DataContext())
            {
                data = await ctx.Professions.ToListAsync();
            }
            return data;
        }

        public async Task<LibraryBasicInfoResp> GetInstitutionInfo(string instituteLink)
        {
            LibraryBasicInfoResp resp = null;
            using (var ctx = new DataContext())
            {
                var inst = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.InstitutionUrl.ToLower() == instituteLink.ToLower() && t.AccountStatus == AccountStatus.ACTIVE);
                inst.CheckExist("Library");

                resp = new LibraryBasicInfoResp();
                resp.Email = inst.Email;
                resp.Id = inst.Id;
                resp.LibraryName = inst.LibraryName;
                resp.PhotoUrl = inst.LogoUrl;
                resp.Website = inst.Website;
                resp.Link = $"{ConfigData.Config.WebUrl}/library/{inst.InstitutionUrl}";
            }

            return resp;
        }
    }
}
