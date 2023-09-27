using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.ApiResponses.B2B;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class EnumOpt
    {
        public int Value { get; set; }
        public string Description { get; set; }
    }
    public class B2BManagementService: IB2BManagement
    {
        private readonly ILogger<B2BManagementService> _logger;
        public B2BManagementService(ILogger<B2BManagementService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsB2BOwnerOfLibrary(int b2bId, int libId)
        {
            using(var ctx  = new DataContext())
            {
                var lib = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libId && t.B2BAccountId == b2bId && t.AccountStatus == AccountStatus.ACTIVE);
                lib.CheckExist("Library");
                return true;
            }
        }


        public async Task UpdateLibrarySub(int libId, int subId, decimal? negotiatedPrice, bool? autoRenew, int? b2bId = null)
        {
            using(var ctx = new DataContext())
            {
                var sub = await ctx.LibrarySubscriptions.Where(t => t.Id == subId).FirstOrDefaultAsync();
                sub.CheckExist("Library Subscription");

                if(sub.LibraryId != libId)
                {
                    throw new CoachOnlineException("Subscription does not belong to selected library", CoachOnlineExceptionState.DataNotValid);
                }

                if(!(sub.Status == LibrarySubscriptionStatus.ACTIVE || sub.Status == LibrarySubscriptionStatus.AWAITING))
                {
                    throw new CoachOnlineException("Cannot change subscription data that is cancelled or ended.", CoachOnlineExceptionState.Expired);
                }
                if (b2bId.HasValue)
                {
                    await IsB2BOwnerOfLibrary(b2bId.Value, sub.LibraryId);
                }

                if (negotiatedPrice.HasValue)
                {
                    sub.NegotiatedPrice = negotiatedPrice;
                }
                if (autoRenew.HasValue)
                {
                    sub.AutoRenew = autoRenew.Value;
                }

                await ctx.SaveChangesAsync();
            }
        }

        public async Task AutoRenewSubscriptions()
        {
            using(var ctx = new DataContext())
            {
                var now = DateTime.Now;
                var subsToProlong = await ctx.LibrarySubscriptions.Where(t => t.AutoRenew == true && (!t.IsProlonged.HasValue || t.IsProlonged.Value == false) && t.Status == LibrarySubscriptionStatus.ACTIVE && t.SubscriptionEnd > now && t.SubscriptionStart< now).ToListAsync();

                foreach(var s in subsToProlong)
                {
                    //check date to sub end
                    var dateWhenProlongShouldWork = s.SubscriptionEnd.AddDays(-3);

                    if(now>= dateWhenProlongShouldWork)
                    {
                        //prolong sub
                        try
                        {
                            await RenewSubscription(s.LibraryId,s.Id);
                        }
                        catch(CoachOnlineException ex)
                        {
                            _logger.LogInformation(ex.Message);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                    }
                }
            }
        }

        public async Task CheckLibrariesSubscriptionStates()
        {
            using (var ctx = new DataContext())
            {
                var libraries = await ctx.LibraryAccounts.Include(s => s.Subscriptions).ToListAsync();
                if (libraries != null)
                {
                    foreach (var l in libraries)
                    {
                        if (l.Subscriptions != null)
                        {
                            var subs = l.Subscriptions.Where(t => t.Status == Model.LibrarySubscriptionStatus.ACTIVE || t.Status == Model.LibrarySubscriptionStatus.AWAITING);
                            foreach (var s in subs)
                            {
                                var now = DateTime.Now;
                                if (s.Status == Model.LibrarySubscriptionStatus.ACTIVE)
                                {
                                    if (s.SubscriptionEnd <= now)
                                    {
                                        s.Status = Model.LibrarySubscriptionStatus.ENDED;
                                    }
                                }
                                else if (s.Status == Model.LibrarySubscriptionStatus.AWAITING)
                                {
                                    if (s.SubscriptionStart <= now && s.SubscriptionEnd >= now)
                                    {
                                        s.Status = Model.LibrarySubscriptionStatus.ACTIVE;
                                    }
                                }
                            }

                            await ctx.SaveChangesAsync();
                        }
                    }
                }
            }
        }

        public async Task RenewSubscription(int libId, int subscriptionId, int? b2bId = null)
        {
            using(var ctx = new DataContext())
            {
                var sub = await ctx.LibrarySubscriptions.FirstOrDefaultAsync(t => t.Id == subscriptionId);
                sub.CheckExist("Library Subscription");

                if (sub.LibraryId != libId)
                {
                    throw new CoachOnlineException("Subscription does not belong to selected library", CoachOnlineExceptionState.DataNotValid);
                }

                if (b2bId.HasValue)
                {
                    await IsB2BOwnerOfLibrary(b2bId.Value, sub.LibraryId);
                }

                if(sub.Status != LibrarySubscriptionStatus.ACTIVE)
                {
                    throw new CoachOnlineException("Cannot renew subscription that is not active", CoachOnlineExceptionState.DataNotValid);
                }

                if(sub.IsProlonged.HasValue && sub.IsProlonged==true)
                {
                    throw new CoachOnlineException("Subscription has been renewed already", CoachOnlineExceptionState.AlreadyChanged);
                }

                var startSub = sub.SubscriptionEnd;
                var endSub = CalculateSubEndDate(sub.SubscriptionEnd, sub.TimePeriod);

                var otherSubs = ctx.LibrarySubscriptions.Where(t => t.LibraryId == sub.LibraryId && t.Status == LibrarySubscriptionStatus.AWAITING).ToList();
                if (otherSubs != null)
                {
                    foreach (var s in otherSubs)
                    {
                        if (!(startSub >= s.SubscriptionEnd || endSub <= s.SubscriptionStart))
                        {
                            throw new CoachOnlineException("Other awaiting subscriptions have overlapping dates. First cancel awaiting subscription.", CoachOnlineExceptionState.DataNotValid);
                        }
                    }
                }

                var newSub = new LibrarySubscription();
                newSub.AccessType = sub.AccessType;
                newSub.AutoRenew = sub.AutoRenew;
                newSub.Currency = sub.Currency;
                newSub.LibraryId = sub.LibraryId;
                newSub.NegotiatedPrice = sub.NegotiatedPrice;
                newSub.NumberOfActiveUsers = sub.NumberOfActiveUsers;
                newSub.Price = sub.Price;
                newSub.PricePlanId = sub.PricePlanId;
                newSub.PricingName = sub.PricingName;
                newSub.Status = LibrarySubscriptionStatus.AWAITING;
                newSub.TimePeriod = sub.TimePeriod;
                newSub.SubscriptionStart = sub.SubscriptionEnd;
                newSub.SubscriptionEnd = CalculateSubEndDate(sub.SubscriptionEnd, sub.TimePeriod);

                ctx.LibrarySubscriptions.Add(newSub);


                sub.IsProlonged = true;
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<int> GetB2BAccountIdByToken(string token)
        {
            using (var ctx = new DataContext())
            {
                var authToken = await ctx.B2BAccountTokens.Where(t => t.Token == token).Include(b => b.B2BAccount).OrderByDescending(t => t.Created).FirstOrDefaultAsync();
                authToken.CheckExist("Auth token");

                if (authToken.Disposed)
                {
                    throw new CoachOnlineException("Token is not active.", CoachOnlineExceptionState.TokenNotActive);
                }

                if (authToken.ValidTo < ConvertTime.ToUnixTimestampLong(DateTime.Now))
                {
                    authToken.Disposed = true;
                    await ctx.SaveChangesAsync();
                    throw new CoachOnlineException("Token is not active.", CoachOnlineExceptionState.TokenNotActive);
                }

                authToken.B2BAccount.CheckExist("B2B Account");
                
                if(authToken.B2BAccount.AccountStatus == AccountStatus.DELETED)
                {
                    throw new CoachOnlineException("B2B Account does not exist.", CoachOnlineExceptionState.NotExist);
                }

                return authToken.B2BAccount.Id;

            }
        }

        public async Task<string> LoginToB2BAccount(string login, string password)
        {
            using(var ctx = new DataContext())
            {
                var account = await ctx.B2BAccounts.Where(t => t.Login.ToLower() == login.ToLower() && t.AccountStatus == AccountStatus.ACTIVE).Include(t=>t.AccessTokens).FirstOrDefaultAsync();
                account.CheckExist("B2B Account");

                var hash = LetsHash.ToSHA512(password);

                if(hash != account.Password)
                {
                    throw new CoachOnlineException("Wrong password provided.", CoachOnlineExceptionState.WrongPassword);
                }

                if (account.AccountStatus == AccountStatus.DELETED)
                {
                    throw new CoachOnlineException("B2B Account does not exist.", CoachOnlineExceptionState.NotExist);
                }

                var newToken = LetsHash.RandomHash(account.Login);

                if(account.AccessTokens == null)
                {
                    account.AccessTokens = new List<B2BAcessToken>();
                }

                account.AccessTokens.Add(new B2BAcessToken { B2BAccountId = account.Id, Token = newToken, Disposed = false, Created = ConvertTime.ToUnixTimestampLong(DateTime.Now), ValidTo = ConvertTime.ToUnixTimestampLong(DateTime.Now.AddDays(30)) });

                await ctx.SaveChangesAsync();

                return newToken;
            }
        }

        public async Task ManageServicesForB2BAccount(int accountId, ManageServicesForB2BAccountRqs rqs)
        {
            using (var ctx = new DataContext())
            {
                var account = await ctx.B2BAccounts.FirstOrDefaultAsync(t => t.Id == accountId && t.AccountStatus == AccountStatus.ACTIVE);
                account.CheckExist("B2B Account");
                if (rqs.Services != null)
                {
                    foreach (var s in rqs.Services)
                    {
                        if(s.RemoveService.HasValue && s.RemoveService.Value)
                        {
                            //remove service
                            var svc = await ctx.B2BAccountServices.FirstOrDefaultAsync(t=>t.ServiceId == s.ServiceId && t.B2BAccountId == account.Id);

                            if(svc != null)
                            {
                                ctx.B2BAccountServices.Remove(svc);
                            }

                        }
                        else
                        {
                            if(string.IsNullOrEmpty(s.ComissionCurrency) || !s.Comission.HasValue)
                            {
                                throw new CoachOnlineException("Comission value not provided", CoachOnlineExceptionState.DataNotValid);
                            }
                            var pricing = await ctx.B2BPricings.FirstOrDefaultAsync(t => t.Id == s.ServiceId);

                            //add service
                            if (pricing != null)
                            {
                                var alredyExists = await ctx.B2BAccountServices.FirstOrDefaultAsync(t => t.ServiceId == pricing.Id && t.B2BAccountId == account.Id);

                                if (alredyExists == null)
                                {
                                    B2BAccountService svc = new B2BAccountService();
                                    svc.B2BAccountId = account.Id;
                                    svc.ServiceId = s.ServiceId;
                                    svc.Comission = s.Comission;
                                    svc.ComissionCurrency = s.ComissionCurrency;
                                    ctx.B2BAccountServices.Add(svc);
                                }
                                else
                                {
                                    alredyExists.Comission = s.Comission;
                                    alredyExists.ComissionCurrency = s.ComissionCurrency;
                                }


                            }
                            
                        }
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        public List<EnumOpt> GetPricingPeriods()
        {
            List<EnumOpt> opts = new List<EnumOpt>();
            var values = (B2BPricingPeriod[])Enum.GetValues(typeof(B2BPricingPeriod));
            foreach(var val in values)
            {
                opts.Add(new EnumOpt() { Description = val.ToString(), Value = (int)val});
            }

            opts = opts.Where(t => t.Value == 3).ToList();

            return opts;
        }

        public List<EnumOpt> GetAccessTypes()
        {
            List<EnumOpt> opts = new List<EnumOpt>();
            var values = (B2BPricingAccessType[])Enum.GetValues(typeof(B2BPricingAccessType));
            foreach (var val in values)
            {
                opts.Add(new EnumOpt() { Description = val.ToString(), Value = (int)val });
            }

            opts = opts.Where(t => t.Value ==1).ToList();

            return opts;
        }

        public async Task<List<B2BPricing>> GetPricings()
        {
            using(var ctx = new DataContext())
            {
                var pricings = await ctx.B2BPricings.ToListAsync();

                return pricings;
            }
        }

        public async Task<int> AddPricingPlan(B2BAddPricingRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                B2BPricing p = new B2BPricing();
                p.Currency = rqs.Currency;
                p.NumberOfActiveUsers = rqs.NumberOfActiveUsers;
                p.Price = rqs.Price;
                p.PricingName = rqs.PricingName;
                p.TimePeriod = rqs.TimePeriod;
                p.AccessType = rqs.AccessType;
                ctx.B2BPricings.Add(p);

                await ctx.SaveChangesAsync();

                return p.Id;
            }
        }

        public async Task UpdatePricingPlan(int planId, B2BUpdatePricingRqs rqs)
        {
            using (var ctx = new DataContext())
            {
                var p = await ctx.B2BPricings.FirstOrDefaultAsync(t => t.Id == planId);
                if(!string.IsNullOrEmpty(rqs.Currency))
                {
                    p.Currency = rqs.Currency;
                }

                if (!string.IsNullOrEmpty(rqs.PricingName))
                {
                    p.PricingName = rqs.PricingName;
                }

                if(rqs.NumberOfActiveUsers.HasValue)
                {
                    p.NumberOfActiveUsers = rqs.NumberOfActiveUsers.Value;
                }

                if (rqs.Price.HasValue)
                {
                    p.Price = rqs.Price.Value;
                }
           
                p.TimePeriod = rqs.TimePeriod;

                p.AccessType = rqs.AccessType;

                await ctx.SaveChangesAsync();

            }
        }

        public async Task RemovePricingPlan(int pricingPlanId)
        {
            using (var ctx = new DataContext())
            {
                var p = await ctx.B2BPricings.Where(t => t.Id == pricingPlanId).FirstOrDefaultAsync();
                p.CheckExist("B2B Pricing Plan");

                var exists = await ctx.B2BAccountServices.AnyAsync(t => t.ServiceId == p.Id);

                if(exists)
                {
                    throw new CoachOnlineException("Pricing plan is assigned to B2B account. Cannot delete.", CoachOnlineExceptionState.CantChange);
                }

                ctx.B2BPricings.Remove(p);

                await ctx.SaveChangesAsync();       
            }
        }

        public async Task<B2BAccountResponseWithAccountType> GetB2BAccount(int accountId)
        {
            using (var ctx = new DataContext())
            {

                var a = await ctx.B2BAccounts.Where(t=>t.Id == accountId && t.AccountStatus == AccountStatus.ACTIVE).Include(s => s.AccountSalesPersons).Include(s => s.AvailableServices).ThenInclude(s => s.Service).FirstOrDefaultAsync();
                a.CheckExist("B2B Account");
    
                    var itm = new B2BAccountResponseWithAccountType();
                    itm.AccountType = LibraryManagementService.B2BAccountType.B2B_ACCOUNT;
                    itm.Id = a.Id;
                    itm.Login = a.Login;
                    itm.Email = a.Email;
                    itm.PhotoUrl = a.LogoUrl;
                    itm.PostalCode = a.PostalCode;
                    itm.Street = a.Street;
                    itm.StreetNo = a.StreetNo;
                    itm.AccountName = a.AccountName;
                    itm.City = a.City;
                    itm.Country = a.Country;
                    itm.Website = a.Website;
                    itm.PhoneNo = a.PhoneNo;
                    itm.ContractSigned = a.ContractSigned;
                    if (itm.ContractSigned)
                    {
                        itm.ContractSignDate = a.ContractSignDate;
                        itm.ComissionCurrency = a.ComissionCurrency;
                        itm.Comission = a.Comission;
                    }
                    if (a.AccountSalesPersons != null)
                    {
                        itm.AccountSalesPersons = new List<B2BSalesPersonResponse>();

                        foreach (var p in a.AccountSalesPersons)
                        {
                            itm.AccountSalesPersons.Add(new B2BSalesPersonResponse() { Email = p.Email, FirstName = p.Fname, Id = p.Id, LastName = p.Lname, PhoneNo = p.PhoneNo, PhotoUrl = p.ProfilePicUrl });
                        }
                    }

                    itm.AvailableServices = new List<B2BAccountServiceResponse>();

                    if (a.AvailableServices != null)
                    {
                        foreach (var s in a.AvailableServices)
                        {
                            B2BAccountServiceResponse accSvc = new B2BAccountServiceResponse();
                            accSvc.ServiceId = s.ServiceId;
                            accSvc.Currency = s.Service.Currency;
                            accSvc.NumberOfActiveUsers = s.Service.NumberOfActiveUsers;
                            accSvc.Price = s.Service.Price;
                            accSvc.PricingName = s.Service.PricingName;
                            accSvc.TimePeriod = s.Service.TimePeriod;
                            accSvc.AccessType = s.Service.AccessType;
                            accSvc.Comission = s.Comission.HasValue ? s.Comission.Value : 0;
                            accSvc.ComissionCurrency = s.ComissionCurrency;
                            itm.AvailableServices.Add(accSvc);
                        }
                    }

                return itm;
            }
        }

        public async Task<List<B2BAccountResponse>> GetB2BAccounts()
        {
            List<B2BAccountResponse> data = new List<B2BAccountResponse>();
            using (var ctx = new DataContext())
            {

                var accounts = await ctx.B2BAccounts.Where(t=>t.AccountStatus == AccountStatus.ACTIVE).Include(s => s.AccountSalesPersons).Include(s=>s.AvailableServices).ThenInclude(s=>s.Service).ToListAsync();

                foreach (var a in accounts)
                {
                    var itm = new B2BAccountResponse();
                    itm.Id = a.Id;
                    itm.Login = a.Login;
                    itm.PhotoUrl = a.LogoUrl;
                    itm.PostalCode = a.PostalCode;
                    itm.Street = a.Street;
                    itm.StreetNo = a.StreetNo;
                    itm.Email = a.Email;
                    itm.AccountName = a.AccountName;
                    itm.City = a.City;
                    itm.PhoneNo = a.PhoneNo;
                    itm.Country = a.Country;
                    itm.Website = a.Website;
                    itm.ContractSigned = a.ContractSigned;
                    if(itm.ContractSigned)
                    {
                        itm.ContractSignDate = a.ContractSignDate;
                        itm.ComissionCurrency = a.ComissionCurrency;
                        itm.Comission = a.Comission;
                    }
                    if(a.AccountSalesPersons != null)
                    {
                        itm.AccountSalesPersons = new List<B2BSalesPersonResponse>();

                        foreach(var p in a.AccountSalesPersons)
                        {
                            itm.AccountSalesPersons.Add(new B2BSalesPersonResponse() { Email = p.Email, FirstName = p.Fname, Id = p.Id, LastName = p.Lname, PhoneNo = p.PhoneNo, PhotoUrl = p.ProfilePicUrl });
                        }
                    }

                    itm.AvailableServices = new List<B2BAccountServiceResponse>();

                    if(a.AvailableServices != null)
                    {
                        foreach(var s in a.AvailableServices)
                        {
                            B2BAccountServiceResponse accSvc = new B2BAccountServiceResponse();
                            accSvc.ServiceId = s.ServiceId;
                            accSvc.Currency = s.Service.Currency;
                            accSvc.NumberOfActiveUsers = s.Service.NumberOfActiveUsers;
                            accSvc.Price = s.Service.Price;
                            accSvc.PricingName = s.Service.PricingName;
                            accSvc.TimePeriod = s.Service.TimePeriod;
                            accSvc.AccessType = s.Service.AccessType;
                            accSvc.Comission = s.Comission.HasValue ? s.Comission.Value : 0;
                            accSvc.ComissionCurrency = s.ComissionCurrency;
                            itm.AvailableServices.Add(accSvc);
                        }
                    }

                    data.Add(itm);
                }
            }

            return data;
        }

        public async Task DeleteB2BAccount(int accountId)
        {
            using(var ctx = new DataContext())
            {
                var b2b = await ctx.B2BAccounts.FirstOrDefaultAsync(t => t.Id == accountId);
                b2b.CheckExist("B2B Account");
                b2b.AccountStatus = AccountStatus.DELETED;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task DeleteLibraryAccount(int accountId)
        {
            using (var ctx = new DataContext())
            {
                var library = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == accountId);
                library.CheckExist("Library");
                library.AccountStatus = AccountStatus.DELETED;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateB2BAccountPassword(int accountId, string secret, string repeat)
        {
            using(var ctx = new DataContext())
            {
                if(secret!=repeat)
                {
                    throw new CoachOnlineException("Passwords don't match", CoachOnlineExceptionState.PasswordsNotMatch);
                }

                var hashed = LetsHash.ToSHA512(secret);

                var account = await ctx.B2BAccounts.FirstOrDefaultAsync(t=>t.Id == accountId && t.AccountStatus == AccountStatus.ACTIVE);
                account.CheckExist("B2B Account");

                account.Password = hashed;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<int> CreateB2BAccount(string login, string password, string repeatPassword)
        {
            using(var ctx = new DataContext())
            {
                if(password != repeatPassword)
                {
                    throw new CoachOnlineException("Passwords don't match.", CoachOnlineExceptionState.DataNotValid);
                }

                if(string.IsNullOrEmpty(password))
                {
                    throw new CoachOnlineException("Passwords cannot be an empty string.", CoachOnlineExceptionState.DataNotValid);
                }
                login = login.Trim();

                if(string.IsNullOrEmpty(login))
                {
                    throw new CoachOnlineException("Login cannot be an empty string.", CoachOnlineExceptionState.DataNotValid);
                }

                var pattern = "^[a-zA-Z0-9]+([_-]?[a-zA-Z0-9])*$";
                if (!Regex.IsMatch(login, pattern))
                {
                    throw new CoachOnlineException("Login can consist only of letters and numbers and - or _ as separators", CoachOnlineExceptionState.DataNotValid);
                }
               

                if(ctx.B2BAccounts.Any(t=>t.Login.ToLower() == login.ToLower() && t.AccountStatus == AccountStatus.ACTIVE))
                {
                    throw new CoachOnlineException("Such login already exists", CoachOnlineExceptionState.AlreadyExist);
                }

                var pass = LetsHash.ToSHA512(password);

                var b2b = new Model.B2BAccount() { Login = login, Password = pass , AccountStatus = AccountStatus.ACTIVE};
                ctx.B2BAccounts.Add(b2b);

                await ctx.SaveChangesAsync();

                return b2b.Id;


            }
        }

        public async Task UpdateB2BAccountInfo(int accountId, UpdateB2BAccountRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var account = await ctx.B2BAccounts.FirstOrDefaultAsync(t => t.Id == accountId && t.AccountStatus == AccountStatus.ACTIVE);
                account.CheckExist("Account");

                if(!string.IsNullOrEmpty(rqs.City))
                {
                    account.City = rqs.City;
                }

                if (!string.IsNullOrEmpty(rqs.Country))
                {
                    account.Country = rqs.Country;
                }

                if(!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    account.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.AccountName))
                {
                    account.AccountName = rqs.AccountName;
                }

                if (!string.IsNullOrEmpty(rqs.PostalCode))
                {
                    account.PostalCode = rqs.PostalCode;
                }

                if (!string.IsNullOrEmpty(rqs.Street))
                {
                    account.Street = rqs.Street;
                }

                if (!string.IsNullOrEmpty(rqs.StreetNo))
                {
                    account.StreetNo = rqs.StreetNo;
                }

                if(!string.IsNullOrEmpty(rqs.Email))
                {
                    account.Email = rqs.Email;
                }

                account.ContractSigned = rqs.ContractSigned;
                if (account.ContractSigned)
                {
                    if (!rqs.Comission.HasValue || string.IsNullOrEmpty(rqs.ComissionCurrency) || !rqs.ContractSignDate.HasValue)
                    {
                        throw new CoachOnlineException("If contract is signed you need to fill information about comission value, currency and sign date.", CoachOnlineExceptionState.DataNotValid);
                    }
                    account.ComissionCurrency = rqs.ComissionCurrency;
                    account.ContractSignDate = rqs.ContractSignDate;
                    account.Comission = rqs.Comission;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());
                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    account.LogoUrl = name + ".jpg";
                }

                if (!string.IsNullOrEmpty(rqs.Website))
                {
                    account.Website = rqs.Website;
                }

             

                await ctx.SaveChangesAsync();
            }
        }

        public async Task AddAccountSalesPerson(int accountId, AddB2BSalesPersonRqs rqs)
        {
            using(var ctx = new DataContext())
            {
                var account = await ctx.B2BAccounts.FirstOrDefaultAsync(t => t.Id == accountId && t.AccountStatus == AccountStatus.ACTIVE);
                account.CheckExist("B2B Account");

                var person = new B2BSalesPerson();
                person.B2BAccountId = account.Id;

                if(string.IsNullOrEmpty(rqs.FirstName) || string.IsNullOrEmpty(rqs.LastName) || string.IsNullOrEmpty(rqs.Email))
                {
                    throw new CoachOnlineException("To create B2B sales person entry you need to provide firt and last names and email.", CoachOnlineExceptionState.WrongDataSent);
                }

                if(!string.IsNullOrEmpty(rqs.Email))
                {
                    person.Email = rqs.Email;
                }

                if (!string.IsNullOrEmpty(rqs.FirstName))
                {
                    person.Fname = rqs.FirstName;
                }

                if (!string.IsNullOrEmpty(rqs.LastName))
                {
                    person.Lname = rqs.LastName;
                }

                if (!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    person.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());

                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    person.ProfilePicUrl = $"{name}.jpg";
                }

                ctx.B2BSalesPersons.Add(person);

                await ctx.SaveChangesAsync();

          
            }
        }

        public async Task RemoveAccountSalesPerson(int salesPersonId)
        {
            using(var ctx = new DataContext())
            {
                var person = await ctx.B2BSalesPersons.FirstOrDefaultAsync(t => t.Id == salesPersonId);
                person.CheckExist("Sales Person");
                ctx.B2BSalesPersons.Remove(person);

                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateAccountSalesPersonInfo(int salesPersonId, AddB2BSalesPersonRqs rqs)
        {
            using (var ctx = new DataContext())
            {
                var person = await ctx.B2BSalesPersons.FirstOrDefaultAsync(t => t.Id == salesPersonId);
                person.CheckExist("Sales Person");
                if (!string.IsNullOrEmpty(rqs.Email))
                {
                    person.Email = rqs.Email;
                }

                if (!string.IsNullOrEmpty(rqs.FirstName))
                {
                    person.Fname = rqs.FirstName;
                }

                if (!string.IsNullOrEmpty(rqs.LastName))
                {
                    person.Lname = rqs.LastName;
                }

                if (!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    person.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());

                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    person.ProfilePicUrl = $"{name}.jpg";
                }

              

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<int> CreateLibraryAccount(int b2bAccountId, CreateLibraryAccountRqs rqs)
        {
           using(var ctx = new DataContext())
            {
                if(rqs.Password != rqs.RepeatPassword)
                {
                    throw new CoachOnlineException("Passwords don't match.", CoachOnlineExceptionState.PasswordsNotMatch);
                }

                bool exists = await ctx.LibraryAccounts.AnyAsync(t => t.Email == rqs.Email && t.AccountStatus == AccountStatus.ACTIVE);

                if(exists)
                {
                    throw new CoachOnlineException("Account with this email already exists",  CoachOnlineExceptionState.AlreadyExist);
                }

                var b2bacc= await ctx.B2BAccounts.FirstOrDefaultAsync(t=>t.Id == b2bAccountId && t.AccountStatus == AccountStatus.ACTIVE);
                b2bacc.CheckExist("B2B Account");
                var pass = LetsHash.ToSHA512(rqs.Password);

                LibraryAccount acc = new LibraryAccount();
                acc.Email = rqs.Email;
                acc.Password = pass;
                acc.B2BAccountId = b2bAccountId;
                acc.AccountStatus = AccountStatus.ACTIVE;
                ctx.LibraryAccounts.Add(acc);
                await ctx.SaveChangesAsync();

                return acc.Id;
            }
        }

        public async Task UpdateLibraryAccountInfo(int libraryAccId, UpdateLibraryAccountRqs rqs, int? b2bAccount = null)
        {
            using(var ctx = new DataContext())
            {
                LibraryAccount account = null;
                if (b2bAccount.HasValue)
                {
                    account = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryAccId && t.B2BAccountId == b2bAccount.Value && t.AccountStatus == AccountStatus.ACTIVE);
                }
                else
                {
                    account = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryAccId && t.AccountStatus == AccountStatus.ACTIVE);
                }
                account.CheckExist("Library");

                if (!string.IsNullOrEmpty(rqs.City))
                {
                    account.City = rqs.City;
                }

                if (!string.IsNullOrEmpty(rqs.Country))
                {
                    account.Country = rqs.Country;
                }

                if (!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    account.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.LibraryName))
                {
                    account.LibraryName = rqs.LibraryName;
                }

                if (!string.IsNullOrEmpty(rqs.PostalCode))
                {
                    account.PostalCode = rqs.PostalCode;
                }

                if (!string.IsNullOrEmpty(rqs.Street))
                {
                    account.Street = rqs.Street;
                }

                if (!string.IsNullOrEmpty(rqs.StreetNo))
                {
                    account.StreetNo = rqs.StreetNo;
                }

                if (!string.IsNullOrEmpty(rqs.Website))
                {
                    account.Website = rqs.Website;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());
                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    account.LogoUrl = name + ".jpg";
                }

                if(rqs.BooksNo.HasValue)
                {
                    account.BooksNo = rqs.BooksNo;
                }
                if(rqs.CdsNo.HasValue)
                {
                    account.CdsNo = rqs.CdsNo;
                }
                if(rqs.ReadersNo.HasValue)
                {
                    account.ReadersNo = rqs.ReadersNo;
                }
                if(rqs.VideosNo.HasValue)
                {
                    account.VideosNo = rqs.VideosNo;
                }
                if(!string.IsNullOrEmpty(rqs.SIGBName))
                {
                    account.SIGBName = rqs.SIGBName;
                }


                await ctx.SaveChangesAsync();
            }
        }

        public async Task AddLibraryReferent(int libraryId, AddLibraryReferentRqs rqs, int? b2bAccount = null)
        {
            using (var ctx = new DataContext())
            {
                LibraryAccount account = null;
                if (b2bAccount.HasValue)
                {
                    account = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.B2BAccountId == b2bAccount.Value && t.AccountStatus == AccountStatus.ACTIVE).Include(i=>i.Referents).FirstOrDefaultAsync();
                }
                else
                {
                    account = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE).Include(i => i.Referents).FirstOrDefaultAsync();
                }

                account.CheckExist("Library");

                if(account.Referents == null)
                {
                    account.Referents = new List<LibraryReferent>();
                }

                var referent = new LibraryReferent();
                referent.LibraryAccountId = account.Id;
                if (string.IsNullOrEmpty(rqs.FirstName) || string.IsNullOrEmpty(rqs.LastName) || string.IsNullOrEmpty(rqs.Email))
                {
                    throw new CoachOnlineException("To create B2B sales person entry you need to provide firt and last names and email.", CoachOnlineExceptionState.WrongDataSent);
                }

                if (!string.IsNullOrEmpty(rqs.Email))
                {
                    referent.Email = rqs.Email;
                }

                if (!string.IsNullOrEmpty(rqs.FirstName))
                {
                    referent.Fname = rqs.FirstName;
                }

                if (!string.IsNullOrEmpty(rqs.LastName))
                {
                    referent.Lname = rqs.LastName;
                }

                if (!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    referent.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());

                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    referent.ProfilePicUrl = $"{name}.jpg";
                }

                ctx.LibraryReferents.Add(referent);

                await ctx.SaveChangesAsync();

            }
        }

        public async Task DeleteLibraryReferent(int referentId, int? b2bAccount = null)
        {
            using (var ctx = new DataContext())
            {
                LibraryReferent referent = null;

                if (b2bAccount.HasValue)
                {
                    referent = await ctx.LibraryReferents.Where(t => t.Id == referentId).Include(i => i.LibraryAccount).Where(i => i.LibraryAccount.B2BAccountId == b2bAccount.Value).FirstOrDefaultAsync();
                }
                else
                {
                    referent = await ctx.LibraryReferents.Where(t => t.Id == referentId).FirstOrDefaultAsync();
                }

                referent.CheckExist("Library referent");

                ctx.LibraryReferents.Remove(referent);

                await ctx.SaveChangesAsync();

            }
        }

        public async Task UpdateLibraryAccountPassword(int libraryId, string secret, string repeat, int? b2bId=null)
        {
            using (var ctx = new DataContext())
            {
                var library = await ctx.LibraryAccounts.FirstOrDefaultAsync(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE);
                library.CheckExist("Library");

                if (b2bId.HasValue && b2bId.Value != library.B2BAccountId)
                {
                    throw new CoachOnlineException("B2B acocunt isn't owner of the library", CoachOnlineExceptionState.PermissionDenied);
                }

                if (secret != repeat)
                {
                    throw new CoachOnlineException("Passwords don't match.", CoachOnlineExceptionState.PasswordsNotMatch);
                }

                var hashed = LetsHash.ToSHA512(secret);

                library.Password = hashed;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task UpdateLibraryReferent(int referentId, AddLibraryReferentRqs rqs, int? b2bAccount = null)
        {
            using (var ctx = new DataContext())
            {
                LibraryReferent referent = null;

                if(b2bAccount.HasValue)
                {
                    referent = await ctx.LibraryReferents.Where(t => t.Id == referentId).Include(i=>i.LibraryAccount).Where(i=>i.LibraryAccount.B2BAccountId == b2bAccount.Value).FirstOrDefaultAsync();
                }
                else
                {
                    referent = await ctx.LibraryReferents.Where(t => t.Id == referentId).FirstOrDefaultAsync();
                }

                referent.CheckExist("Library referent");

                if (!string.IsNullOrEmpty(rqs.Email))
                {
                    referent.Email = rqs.Email;
                }

                if (!string.IsNullOrEmpty(rqs.FirstName))
                {
                    referent.Fname = rqs.FirstName;
                }

                if (!string.IsNullOrEmpty(rqs.LastName))
                {
                    referent.Lname = rqs.LastName;
                }

                if (!string.IsNullOrEmpty(rqs.PhoneNo))
                {
                    referent.PhoneNo = rqs.PhoneNo;
                }

                if (!string.IsNullOrEmpty(rqs.PhotoBase64))
                {
                    var name = LetsHash.RandomHash(DateTime.Now.ToString());

                    await Extensions.SaveImageAsync(rqs.PhotoBase64, name);

                    referent.ProfilePicUrl = $"{name}.jpg";
                }

                ctx.LibraryReferents.Add(referent);

                await ctx.SaveChangesAsync();

            }
        }

        public async Task<List<B2BLibraryResponse>> GetB2BAccountClients(int b2bAccountId)
        {
            List<B2BLibraryResponse> data = new List<B2BLibraryResponse>();
            using (var ctx = new DataContext())
            {
                var clients = await ctx.LibraryAccounts.Where(t => t.B2BAccountId == b2bAccountId && t.AccountStatus == AccountStatus.ACTIVE).ToListAsync();

                foreach(var c in clients)
                {
                    var resp = await GetLibraryInfo(c);

                    data.Add(resp);
                }
            }

            return data;
        }

        public async Task<List<B2BLibraryResponse>> GetAllLibraries()
        {
            List<B2BLibraryResponse> data = new List<B2BLibraryResponse>();
            using (var ctx = new DataContext())
            {
                var clients = await ctx.LibraryAccounts.Where(t=> t.AccountStatus == AccountStatus.ACTIVE).ToListAsync();

                foreach (var c in clients)
                {
                    var resp = await GetLibraryInfo(c);

                    data.Add(resp);
                }
            }

            return data;
        }

        public async Task<B2BLibraryResponse> GetLibraryAccount(int libraryId, int? b2bAccount = null)
        {
            using(var ctx = new DataContext())
            {
                LibraryAccount account = null;

                if(b2bAccount.HasValue)
                {
                    account = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.B2BAccountId == b2bAccount.Value && t.AccountStatus == AccountStatus.ACTIVE).FirstOrDefaultAsync();
                }
                else
                {
                    account = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE).FirstOrDefaultAsync();
                }

                account.CheckExist("Library");

                return await GetLibraryInfo(account);
            }
        }

        private async Task<B2BLibraryResponse> GetLibraryInfo(LibraryAccount account)
        {
            B2BLibraryResponse resp= new B2BLibraryResponse();
            resp.City = account.City;
            resp.InstitutionLink = account.InstitutionUrl;
            resp.B2BAccountId = account.B2BAccountId;
            resp.Country = account.Country;
            resp.Email = account.Email;
            resp.Id = account.Id;
            resp.LibraryName = account.LibraryName;
            resp.PhotoUrl = account.LogoUrl;
            resp.PostalCode = account.PostalCode;
            resp.Street = account.Street;
            resp.PhoneNo = account.PhoneNo;
            resp.StreetNo = account.StreetNo;
            resp.Website = account.Website;
            resp.BooksNo = account.BooksNo;
            resp.CdsNo = account.CdsNo;
            resp.ReadersNo = account.ReadersNo;
            resp.SIGBName = account.SIGBName;
            resp.VideosNo = account.VideosNo;
            resp.Link = string.IsNullOrEmpty(account.InstitutionUrl) ? "" : $"{ConfigData.Config.WebUrl}/library/{account.InstitutionUrl}";

            resp.referents = new List<LibraryReferentResponse>();
          
            resp.AllSubscriptions = new List<LibrarySubscriptionResponse>();

            using(var ctx = new DataContext())
            {
                var b2bAccount = ctx.B2BAccounts.Where(t => t.Id == account.B2BAccountId).FirstOrDefault();
                if(b2bAccount != null)
                {
                    resp.B2BAccountName = b2bAccount.AccountName;
                }
                var referents = await ctx.LibraryReferents.Where(t => t.LibraryAccountId == account.Id).ToListAsync();

                if(referents!= null)
                {
                    foreach(var r in referents)
                    {
                        resp.referents.Add(new LibraryReferentResponse() { Email = r.Email, FirstName = r.Fname, Id = r.Id, LastName = r.Lname, PhoneNo = r.PhoneNo, PhotoUrl = r.ProfilePicUrl });
                    }
                }

                var subscriptions = await ctx.LibrarySubscriptions.Where(t => t.LibraryId == account.Id).ToListAsync();

                if(subscriptions != null)
                {
                    foreach(var s in subscriptions)
                    {
                        resp.AllSubscriptions.Add(new LibrarySubscriptionResponse()
                        {
                            Id = s.Id,
                            AccessType = s.AccessType,
                            Currency = s.Currency,
                            IsActive = s.Status == LibrarySubscriptionStatus.ACTIVE?true:false,
                            NegotiatedPrice = s.NegotiatedPrice,
                            Status = s.Status,
                            NumberOfActiveUsers = s.NumberOfActiveUsers,
                            Price = s.Price,
                            PricePlanId = s.PricePlanId,
                            PricingName = s.PricingName,
                            SubscriptionEnd = s.SubscriptionEnd,
                            SubscriptionStart = s.SubscriptionStart,
                            TimePeriod = s.TimePeriod
                        });

                        if(s.Status == LibrarySubscriptionStatus.ACTIVE)
                        {
                            resp.ActiveSubscription = new LibrarySubscriptionResponse()
                            {
                                Id = s.Id,
                                AccessType = s.AccessType,
                                Currency = s.Currency,
                                IsActive = s.Status == LibrarySubscriptionStatus.ACTIVE ? true : false,
                                NegotiatedPrice = s.NegotiatedPrice,
                                Status = s.Status,    
                                NumberOfActiveUsers = s.NumberOfActiveUsers,
                                Price = s.Price,
                                PricePlanId = s.PricePlanId,
                                PricingName = s.PricingName,
                                SubscriptionEnd = s.SubscriptionEnd,
                                SubscriptionStart = s.SubscriptionStart,
                                TimePeriod = s.TimePeriod
                            };
                        }
                    }

                    
                }
            }

            return resp;
        }

        public async Task<string> GenerateInstitutionLink(int libraryId, string proposition, int? b2bAccount = null)
        {
            using (var ctx = new DataContext())
            {
                LibraryAccount lib = null;
                if (b2bAccount.HasValue)
                {
                    lib = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.B2BAccountId == b2bAccount.Value && t.AccountStatus == AccountStatus.ACTIVE).Include(p => p.Subscriptions).FirstOrDefaultAsync();
                }
                else
                {
                    lib = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE).Include(p => p.Subscriptions).FirstOrDefaultAsync();
                }
                lib.CheckExist("Library");

                proposition = proposition.Trim().Replace(" ", "");

                if (string.IsNullOrEmpty(proposition))
                {
                    throw new CoachOnlineException("The proposed name cannot be empty", CoachOnlineExceptionState.WrongDataSent);
                }

            

                var exists = await ctx.LibraryAccounts.AnyAsync(t => t.InstitutionUrl == proposition);
                if(exists)
                {
                    throw new CoachOnlineException("Such institution link already exists.", CoachOnlineExceptionState.AlreadyExist);
                }

                lib.InstitutionUrl = proposition;

                await ctx.SaveChangesAsync();

                return lib.InstitutionUrl;
            }
        }

        public async Task CancelLibraryPricingPlan(int subId, int? b2bAccount = null)
        {
            using (var ctx = new DataContext())
            {
                LibrarySubscription sub = null;
                if (b2bAccount.HasValue)
                {
                    sub = await ctx.LibrarySubscriptions.Where(t => t.Id == subId).Include(l => l.Library).Where(l => l.Library.B2BAccountId == b2bAccount.Value).FirstOrDefaultAsync();
                }
                else
                {
                    sub = await ctx.LibrarySubscriptions.Where(t => t.Id == subId).FirstOrDefaultAsync();
                }
                sub.CheckExist("Library subscription");

                sub.Status = LibrarySubscriptionStatus.CANCELLED;

                await ctx.SaveChangesAsync();
            }
        }

        public async Task AssignPricingPlanToLibrary(int libraryId, int planId, DateTime start, decimal? negotiatedPrice, bool autoRenew, int? b2bAccount = null)
        {
            using(var ctx = new DataContext())
            {
                LibraryAccount lib = null;
                if (b2bAccount.HasValue)
                {
                    lib = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.B2BAccountId == b2bAccount.Value && t.AccountStatus == AccountStatus.ACTIVE).Include(p=>p.Subscriptions).FirstOrDefaultAsync();
                }
                else
                {
                    lib = await ctx.LibraryAccounts.Where(t => t.Id == libraryId && t.AccountStatus == AccountStatus.ACTIVE).Include(p => p.Subscriptions).FirstOrDefaultAsync();
                }
                lib.CheckExist("Library");

                var pricePlan = await ctx.B2BPricings.FirstOrDefaultAsync(t => t.Id == planId);
                pricePlan.CheckExist("B2B pricing");

                if(lib.Subscriptions==null)
                {
                    lib.Subscriptions = new List<LibrarySubscription>();
                }

                if(start < DateTime.UtcNow.AddMinutes(-5))
                {
                    throw new CoachOnlineException("Subscription start date cannot be in the past.", CoachOnlineExceptionState.WrongDataSent);
                }

                var end = CalculateSubEndDate(start, pricePlan.TimePeriod);
                ///to do check if any active
                var otherSubs = lib.Subscriptions.Where(t => t.Status == LibrarySubscriptionStatus.ACTIVE || t.Status == LibrarySubscriptionStatus.AWAITING).ToList();
                if (otherSubs != null)
                {
                    foreach(var s in otherSubs)
                    {
                        if(!(start>= s.SubscriptionEnd || end<= s.SubscriptionStart))
                        {
                            throw new CoachOnlineException("Other active or awaiting subscriptions have overlapping dates. First cancel active or awaiting subscription.", CoachOnlineExceptionState.DataNotValid);
                        }
                    }
                }

                LibrarySubscription sub = new LibrarySubscription();
                
                sub.AccessType = pricePlan.AccessType;
                sub.Currency = pricePlan.Currency;
                sub.LibraryId = lib.Id;
                sub.NumberOfActiveUsers = pricePlan.NumberOfActiveUsers;
                sub.Price = pricePlan.Price;
                sub.PricingName = pricePlan.PricingName;
                sub.TimePeriod = pricePlan.TimePeriod;
                sub.PricePlanId = pricePlan.Id;
                sub.SubscriptionStart = start;
                sub.SubscriptionEnd = end;
                sub.AutoRenew = autoRenew;
                sub.Status = IsSubActive(sub.SubscriptionStart, sub.SubscriptionEnd)? LibrarySubscriptionStatus.ACTIVE: LibrarySubscriptionStatus.AWAITING ;
                sub.NegotiatedPrice = negotiatedPrice.HasValue ? negotiatedPrice.Value : pricePlan.Price;
                lib.Subscriptions.Add(sub);

                await ctx.SaveChangesAsync();

            }
        }

        public async Task CancelLibrarySubscription(int subscriptionId, int? b2bId = null)
        {
            using(var ctx = new DataContext())
            {
                var sub = await ctx.LibrarySubscriptions.Where(t => t.Id == subscriptionId).FirstOrDefaultAsync();
                sub.CheckExist("Subscription");
                if(b2bId.HasValue)
                {
                    await IsB2BOwnerOfLibrary(b2bId.Value, sub.LibraryId);
                }

                if(sub.Status == LibrarySubscriptionStatus.CANCELLED || sub.Status == LibrarySubscriptionStatus.ENDED)
                {
                    throw new CoachOnlineException($"Cannot cancel subscription in {sub.Status.ToString()} state.", CoachOnlineExceptionState.CantChange);
                }

                sub.Status = LibrarySubscriptionStatus.CANCELLED;

                await ctx.SaveChangesAsync();

            }
        }

        private DateTime CalculateSubEndDate(DateTime start, B2BPricingPeriod period)
        {
            if(period == B2BPricingPeriod.MONTHLY)
            {
                return start.AddMonths(1);
            }
            else if(period == B2BPricingPeriod.QUARTERLY)
            {
                return start.AddMonths(3);
            }
            else if (period == B2BPricingPeriod.WEEKLY)
            {
                return start.AddDays(7);
            }
            else if (period == B2BPricingPeriod.YEARLY)
            {
                return start.AddYears(1);
            }
            return new DateTime();
        }

        public bool IsSubActive(DateTime start, DateTime end)
        {
            if (start <= DateTime.Now && DateTime.Now <= end)
                return true;
            else return false;
        }
    }
}
