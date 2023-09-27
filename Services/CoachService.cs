using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Coach;
using CoachOnline.PayPalIntegration;
using CoachOnline.Statics;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class CoachService: ICoach
    {
        private readonly IBalance _balanceSvc;
  
        public CoachService(IBalance balanceSvc)
        {
            _balanceSvc = balanceSvc;
        }


        public async Task<List<CoachSummarizedRankingReponse>> GetCurrentCoachRanking(int userId, int? month = null, int? year = null)
        {
            var coach = await GetCoachByIdAsync(userId);
            coach.CheckExist("Coach");

            DateTime? dt = null;
            if (month.HasValue && year.HasValue)
            {
                dt = new DateTime(year.Value, month.Value, 1);
            }
            var data = new List<CoachSummarizedRankingReponse>();

            using (var ctx = new DataContext())
            {
                List<MonthlyBalance> dbBalances = null;
                if (dt.HasValue)
                {
                    var dtForBalance = dt.Value.AddMonths(-1);
                    dbBalances = await ctx.MonthlyBalances.Where(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month).ToListAsync();
                }
                else
                {
                    dbBalances = await ctx.MonthlyBalances.ToListAsync();
                }


                List<CoachBalanceMonth> monthBalanceData = new List<CoachBalanceMonth>();
                //gather all results
                foreach (var dbBalance in dbBalances)
                {
                    var localList = await ctx.CoachMonthlyBalance.Where(t => t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).ToListAsync();

                    monthBalanceData = monthBalanceData.Union(localList).ToList();
                }

                if (monthBalanceData != null)
                {

                    var grouppedByCoach = monthBalanceData.GroupBy(t => t.CoachId).ToList();

                    foreach (var grp in grouppedByCoach)
                    {
                        var coachTemp = await GetCoachByIdAsync(grp.Key);
                        if (coachTemp != null && coachTemp.Status != Model.UserAccountStatus.DELETED)
                        {
                            var coachBalanceResponse = new CoachSummarizedRankingReponse();
                            coachBalanceResponse.Month = dt;
                            coachBalanceResponse.CoachId = grp.Key;
                            coachBalanceResponse.Name = $"{coachTemp.FirstName} {coachTemp.Surname}";
                            coachBalanceResponse.Category = coachTemp.AccountCategories != null ? string.Join(",", coachTemp.AccountCategories.Select(t => t.Name)) : "";
                            coachBalanceResponse.JoinDate = coachTemp.AccountCreationDate;
                            coachBalanceResponse.TotalMinutes = 0;
                            var dayBalances = grp.Select(x => x.DayBalances).ToList();
                            if (dayBalances != null)
                            {
                                dayBalances.ForEach(el =>
                                {
                                    coachBalanceResponse.TotalMinutes += Math.Round(el.Sum(x => x.TotalEpisodesWatchTime) / 60,2);
                                });

                               // coachBalanceResponse.TotalMinutes = dayBalances[0].Sum(t=>t.) //Math.Round(dayBalances.Select(t=>t.to).Sum(t => t) / 60, 2);
                            }

                            if (grp.Key == coach.Id)
                            {
                                coachBalanceResponse.IsMe = true;
                            }

                            data.Add(coachBalanceResponse);
                        }
                    }
                }

                data = data.OrderByDescending(t => t.TotalMinutes).ThenBy(c => c.CoachId).ToList();

                int rank = 1;
                foreach (var x in data)
                {
                    x.RankPosition = rank;
                    rank++;
                }

                var myPosition = data.FirstOrDefault(x => x.IsMe);
                if (myPosition != null && myPosition.RankPosition > 10)
                {
                    data = data.Take(10).ToList();
                    data.Add(myPosition);
                }
                else
                {
                    data = data.Take(10).ToList();
                }

            
            }

            return data;
        }


        public async Task<List<CoachSummarizedRankingReponse>> GetCurrentCoachesRankingAll()
        {

            var data = new List<CoachSummarizedRankingReponse>();

            using (var ctx = new DataContext())
            {
                List<MonthlyBalance> dbBalances = await ctx.MonthlyBalances.ToListAsync();
                
                List<CoachBalanceMonth> monthBalanceData = new List<CoachBalanceMonth>();
                //gather all results
                foreach (var dbBalance in dbBalances)
                {
                    var localList = await ctx.CoachMonthlyBalance.Where(t => t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).ToListAsync();

                    monthBalanceData = monthBalanceData.Union(localList).ToList();
                }

                if (monthBalanceData != null)
                {

                    var grouppedByCoach = monthBalanceData.GroupBy(t => t.CoachId).ToList();

                    foreach (var grp in grouppedByCoach)
                    {
                        var coachTemp = await GetCoachByIdAsync(grp.Key);
                        if (coachTemp != null && coachTemp.Status != Model.UserAccountStatus.DELETED)
                        {
                            var coachBalanceResponse = new CoachSummarizedRankingReponse();
                            coachBalanceResponse.CoachId = grp.Key;
                            coachBalanceResponse.Name = $"{coachTemp.FirstName} {coachTemp.Surname}";
                            coachBalanceResponse.Category = coachTemp.AccountCategories != null ? string.Join(",", coachTemp.AccountCategories.Select(t => t.Name)) : "";
                            coachBalanceResponse.JoinDate = coachTemp.AccountCreationDate;
                            coachBalanceResponse.TotalMinutes = 0;
                            var dayBalances = grp.Select(x => x.DayBalances).ToList();
                            if (dayBalances != null)
                            {
                                dayBalances.ForEach(el =>
                                {
                                    coachBalanceResponse.TotalMinutes += Math.Round(el.Sum(x => x.TotalEpisodesWatchTime) / 60, 2);
                                });
                            }


                            data.Add(coachBalanceResponse);
                        }
                    }
                }

                data = data.OrderByDescending(t => t.TotalMinutes).ThenBy(c => c.CoachId).ToList();

                int rank = 1;
                foreach (var x in data)
                {
                    x.RankPosition = rank;
                    rank++;
                }
            }

            return data;
        }

        public async Task<List<CategoryAPI>> GetParentCategoriesForSuggestion()
        {
            using(var ctx = new DataContext())
            {
                var categories = await ctx.courseCategories.Where(t => !t.ParentId.HasValue).ToListAsync();
                var categoryList = new List<CategoryAPI>();
                foreach(var c in categories)
                {
                    CategoryAPI r = new CategoryAPI();
                    r.AdultOnly = c.AdultOnly;
                    r.Id = c.Id;
                    r.Name = c.Name;
                    categoryList.Add(r);
                }

                return categoryList;
            }
        }

        public async Task SuggestNewCategory(int userId, string categoryName, bool hasParent, int? parentId, bool adultOnly)
        {
            using(var ctx = new DataContext())
            {
                var exists = await ctx.PendingCategories.AnyAsync(x => x.CategoryName == categoryName && x.CreatedByUserId == userId);
                if(exists)
                {
                    throw new CoachOnlineException("You already suggested such category.", CoachOnlineExceptionState.AlreadyExist);
                }

                var catExists = await ctx.courseCategories.AnyAsync(t => t.Name == categoryName && t.ParentId == parentId);
                if(catExists)
                {
                    throw new CoachOnlineException("Such category already exists.", CoachOnlineExceptionState.AlreadyExist);
                }

                var c = new PendingCategory();
                c.CategoryName = categoryName;
                c.CreatedByUserId = userId;
                c.State = PendingCategoryState.PENDING;
                c.ParentId = null;
                c.AdultOnly = adultOnly;

                if(hasParent && !parentId.HasValue)
                {
                    throw new CoachOnlineException("Parent category id was not provided", CoachOnlineExceptionState.DataNotValid);
                }
                
                if(hasParent)
                {
                    var parentCat = await ctx.courseCategories.FirstOrDefaultAsync(t => t.Id == parentId.Value && !t.ParentId.HasValue);
                    if(parentCat == null)
                    {
                        throw new CoachOnlineException("Parent category does not exist.", CoachOnlineExceptionState.NotExist);
                    }
                    c.ParentId = parentCat.Id;
                
                }

                ctx.PendingCategories.Add(c);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<CurrentPayTransferValueResponse> GetCurrentAmountToWidthraw(User coach)
        {
            CheckUserIsCorrect(coach, false);
            var balanceResp = new CurrentPayTransferValueResponse();
            balanceResp.Balances = new List<MonthCoachBalanceResp>();
            using (var ctx = new DataContext())
            {
                var coachBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id).Include(m=>m.MonthlyBalance).Include(x => x.DayBalances).ToListAsync();

                decimal sum = 0;
                decimal withdrawn = 0;
                decimal toWidhdraw = 0;
                var today = DateTime.Today;
                coachBalance.ForEach(b =>
                {
                    if (b.Month == today.Month && b.Year == today.Year)
                    {
                        sum += b.DayBalances.Where(t => t.Calculated).Sum(t => Math.Round(t.BalanceValue/100,2));
                    }
                    withdrawn += b.DayBalances.Where(t => t.Calculated && t.Transferred).Sum(t => Math.Round(t.BalanceValue/100,2));
                    toWidhdraw += b.DayBalances.Where(t => t.Calculated && !t.Transferred && 
                    ((!t.PayoutViaPaypal.HasValue || t.PayoutViaPaypal.Value == false) || (t.PayoutViaPaypal.HasValue && t.PayoutViaPaypal.Value && t.PayPalPayoutId == null))).Sum(t => Math.Round(t.BalanceValue/100,2));
                    balanceResp.Currency = b.MonthlyBalance.Currency;
                    balanceResp.Balances.Add(new MonthCoachBalanceResp()
                    {
                        Currency = b.MonthlyBalance.Currency,
                        BalanceId = b.Id,
                        DailyBalances = b.DayBalances.Where(t => t.Calculated && !t.Transferred).ToList(),
                        Month = b.Month,
                        Year = b.Year,
                        BalanceValue = b.DayBalances.Where(t => t.Calculated && !t.Transferred).Sum(t => t.BalanceValue),
                        BalanceValueStripe = b.DayBalances.Where(t => t.Calculated && !t.Transferred && (!t.PayoutViaPaypal.HasValue || !t.PayoutViaPaypal.Value)).Sum(t => t.BalanceValue),
                        BalanceValuePaypal = b.DayBalances.Where(t => t.Calculated && !t.Transferred && t.PayoutViaPaypal.HasValue && t.PayoutViaPaypal.Value).Sum(t => t.BalanceValue)
                    }) ;
                    
                });

                balanceResp.CoachId = coach.Id;
                balanceResp.ToWidthraw = toWidhdraw;
                balanceResp.CurrentMonthTotalBalance = sum;
                balanceResp.WithdrawnAmount = withdrawn;
               

            }

            return balanceResp;
        }

        public async Task WithdrawPayment(User coach)
        {
            CheckUserIsCorrect(coach, true);
            var data = await GetCurrentAmountToWidthraw(coach);

            if(data.TotalAmountToWidhdraw<=0)
            {
                throw new CoachOnlineException("Payout value must be greater than 0.", CoachOnlineExceptionState.DataNotValid);
            }

            foreach(var d in data.Balances)
            {
                
                if(d.BalanceValueStripe >0)
                {
                    using (var ctx = new DataContext())
                    {
                        foreach (var x in d.DailyBalances)
                        {
                            var dB = await ctx.CoachDailyBalance.FirstOrDefaultAsync(e => e.Id == x.Id);
                            if (dB != null && (!dB.PayoutViaPaypal.HasValue || dB.PayoutViaPaypal.Value == false))
                            {
                                dB.Transferred = true;
                                dB.TransferDate = DateTime.Now;
                            }
                        }
                        await ctx.SaveChangesAsync();
                        await ProcessPaymentToAccount((long)d.BalanceValueStripe, d.Description, coach.StripeAccountId, d.Currency);
            
                    }
                }
            }

        }

        public async Task WithdrawPaymentViaPayPal(User coach)
        {
            CheckUserIsCorrect(coach, true);
            var data = await GetCurrentAmountToWidthraw(coach);

            foreach (var d in data.Balances)
            {
                if (d.Amonut > 0)
                {
                    using (var ctx = new DataContext())
                    {
                        foreach (var x in d.DailyBalances)
                        {
                            var dB = await ctx.CoachDailyBalance.FirstOrDefaultAsync(e => e.Id == x.Id);
                            if (dB != null)
                            {
                                dB.Transferred = true;
                                dB.TransferDate = DateTime.Now;
                            }
                        }
                        await ctx.SaveChangesAsync();
                        await ProcessPaymentToAccount((long)d.BalanceValue, d.Description, coach.StripeAccountId, d.Currency);

                    }
                }
            }

        }


        public async Task<AccountDataResponse> GetAccountData(User coach)
        {
            CheckUserIsCorrect(coach);
            var resp = new AccountDataResponse();
            using (var ctx = new DataContext())
            {
                var info = await ctx.users.Where(t => t.Id == coach.Id).Include(t => t.companyInfo).FirstOrDefaultAsync();
                var compInfo = info.companyInfo;
                if (compInfo != null)
                {
                    resp.BankAccountNo = compInfo.BankAccountNumber;
                    resp.City = compInfo.City;
                    resp.CompanyName = compInfo.Name;
                    resp.Country = compInfo.Country;
                    resp.SiretNo = compInfo.SiretNumber;
                    resp.StreetNo = compInfo.RegisterAddress;
                    resp.VatNo = compInfo.VatNumber;
                }
            }

            return resp;
        }

        private async Task ProcessPaymentToAccount(long amount, string description, string account_id, string currency)
        {
            var options = new TransferCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Destination = account_id,
                Description = description,
            };

            var service = new TransferService();
            var Transfer = await service.CreateAsync(options);
        }


        public async Task<User> GetCoachByIdAsync(int id)
        {
            User u = null;

            using (var cnx = new DataContext())
            {
                var user = await cnx.users
                    .Where(x => x.Id == id).Include(ac=>ac.AccountCategories)
                    .FirstOrDefaultAsync(); 
                if (user == null || user.UserRole != UserRoleType.COACH)
                {
                    return null;
                }

                u = user;
            }

            return u.WithoutPassword();
        }


        public async Task<User> GetCoachByTokenAsync(string token)
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

                if(user.UserRole != UserRoleType.COACH)
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.WrongDataSent);
                }
                u = user;
            }

            return u.WithoutPassword();
        }

        private bool CheckUserIsCorrect(User u, bool fullcheck=true)
        {
            if (u.UserRole != UserRoleType.COACH)
            {
                throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
            }
            if (fullcheck)
            {
                if (string.IsNullOrEmpty(u.StripeAccountId))
                {
                    throw new CoachOnlineException("Stripe Account is not created", CoachOnlineExceptionState.NotExist);
                }
            
                if (!u.PaymentsEnabled || !u.WithdrawalsEnabled)
                {
                    throw new CoachOnlineException("Account is not verified", CoachOnlineExceptionState.PermissionDenied);
                }
            }

            return true;
        }
    }
}
