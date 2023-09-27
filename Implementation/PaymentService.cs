using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IWebhook _webhookSvc;

        public PaymentService(IWebhook webhookSvc)
        {
            StripeConfiguration.ApiKey = ConfigData.Config.StripeRk;
            _webhookSvc = webhookSvc;

        }

        public async Task<string> GenerateConnectedAccount(string AuthToken, string countryCode)
        {
            string AccountId = "";
            AccountCreateOptions options = new AccountCreateOptions();
            options.Type = "express";
            int id = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == id).FirstOrDefault();
                CheckExistAuth(user, "User");
                if (user.UserRole != Model.UserRoleType.COACH)
                {
                    throw new CoachOnlineException("Cannot create Stripe account.", CoachOnlineExceptionState.DataNotValid);
                }
                if (!string.IsNullOrEmpty(user.StripeAccountId))
                {
                    throw new CoachOnlineException("User already has Stripe Account.", CoachOnlineExceptionState.AlreadyChanged);
                }
                options.BusinessType = "company";
                options.Email = user.EmailAddress;
                if (!string.IsNullOrEmpty(countryCode))
                {
                    options.Country = countryCode;
                }


                options.Company = new AccountCompanyOptions
                {
                    Name = user.FirstName,
                    //Address = new AddressOptions { 
                    //    Country = "FR" }
                };
                options.DefaultCurrency = "EUR";
                options.Capabilities = new AccountCapabilitiesOptions { Transfers = new AccountCapabilitiesTransfersOptions { Requested = true } };
                var service = new AccountService();
                var response = await service.CreateAsync(options);
                user.StripeAccountId = response.Id;
                await cnx.SaveChangesAsync();
            }
            return AccountId;
        }

        public async Task<string> GenerateConnectedAccountAsPrivateUser(string AuthToken, string countryCode)
        {
            string AccountId = "";
            AccountCreateOptions options = new AccountCreateOptions();
            options.Type = "express";
            int id = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == id).FirstOrDefault();
                CheckExistAuth(user, "User");
                if (user.UserRole != Model.UserRoleType.COACH)
                {
                    throw new CoachOnlineException("Cannot create Stripe account.", CoachOnlineExceptionState.DataNotValid);
                }
                if (!string.IsNullOrEmpty(user.StripeAccountId))
                {
                    throw new CoachOnlineException("User already has Stripe Account.", CoachOnlineExceptionState.AlreadyChanged);
                }
                options.BusinessType = "individual";
                options.Email = user.EmailAddress;
                if (!string.IsNullOrEmpty(countryCode))
                {
                    options.Country = countryCode;
                }

                options.Individual = new AccountIndividualOptions
                {
                    Email = user.EmailAddress,
                    FirstName = user.FirstName
                };
                options.DefaultCurrency = "EUR";
                options.Capabilities = new AccountCapabilitiesOptions { Transfers = new AccountCapabilitiesTransfersOptions { Requested = true } };
                var service = new AccountService();
                var response = await service.CreateAsync(options);
                user.StripeAccountId = response.Id;
                await cnx.SaveChangesAsync();
            }
            return AccountId;
        }
        public async Task<string> VerificationFirstStage(string AuthToken)
        {
            string VerifyUrl = "";
            int id = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == id).FirstOrDefault();
                CheckExistAuth(user, "User");
                if (string.IsNullOrEmpty(user.StripeAccountId))
                {
                    throw new CoachOnlineException("User has to create stripe express account first.", CoachOnlineExceptionState.CantChange);
                }

                Stripe.AccountService acnt = new AccountService();
                var account = await acnt.GetAsync(user.StripeAccountId);

                var usr = await _webhookSvc.ChangeUserState(account);
                usr.CheckExist("User");
                if (usr.PaymentsEnabled)
                {
                    throw new CoachOnlineException("User payments already enabled", CoachOnlineExceptionState.CantChange);
                }
                string accountId = user.StripeAccountId;
                var options = new AccountLinkCreateOptions
                {
                    Account = accountId,
                    RefreshUrl = $"{ConfigData.Config.WebUrl}/billing",
                    ReturnUrl = $"{ConfigData.Config.WebUrl}/billing",
                    Type = "account_onboarding",
                };
                var service = new AccountLinkService();
                var accountLink = service.Create(options);
                VerifyUrl = accountLink.Url;
            }
            return VerifyUrl;
        }
        public async Task<string> VerificationKYCStage(string AuthToken)
        {
            string VerifyUrl = "";
            int id = await GetUserIdForTokenAsync(AuthToken);
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.Id == id).FirstOrDefault();
                CheckExistAuth(user, "User");
                if (string.IsNullOrEmpty(user.StripeAccountId))
                {
                    throw new CoachOnlineException("User has to create stripe express account first.", CoachOnlineExceptionState.CantChange);
                }

                Stripe.AccountService acnt = new AccountService();
                var account = await acnt.GetAsync(user.StripeAccountId);

                var usr = await _webhookSvc.ChangeUserState(account);
                usr.CheckExist("User");

                if (!usr.PaymentsEnabled)
                {
                    throw new CoachOnlineException("You have to do first stage first!", CoachOnlineExceptionState.CantChange);
                }

                if (usr.WithdrawalsEnabled)
                {
                    throw new CoachOnlineException("User withdrawals already enabled", CoachOnlineExceptionState.CantChange);
                }
                string accountId = user.StripeAccountId;
                var options = new AccountLinkCreateOptions
                {
                    Account = accountId,
                    RefreshUrl = $"{ConfigData.Config.WebUrl}/billing",
                    ReturnUrl = $"{ConfigData.Config.WebUrl}/billing",
                    Type = "account_onboarding",
                };
                var service = new AccountLinkService();
                var accountLink = service.Create(options);
                VerifyUrl = accountLink.Url;
            }
            return VerifyUrl;
        }
        private void CheckExistAuth(object obj, string FieldName)
        {
            if (obj == null)
            {
                ///Log.Error($"{FieldName} does not exist.");
                throw new CoachOnlineException($"{FieldName} does not exist.", CoachOnlineExceptionState.NotExist);
            }
        }
        public async Task<int> GetUserIdForTokenAsync(string token)
        {
            int id = 0;
            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Include(x => x.UserLogins)
                    .Where(x => x.UserLogins.Any(x => x.AuthToken == token))
                    .FirstOrDefaultAsync();
                if (user == null)
                {
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
                id = user.Id;
            }
            return id;
        }
    }
}
