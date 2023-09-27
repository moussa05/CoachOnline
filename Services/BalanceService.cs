using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class BalanceService : IBalance
    {
        private readonly ILogger<BalanceService> _logger;
        public BalanceService(ILogger<BalanceService> logger)
        {
            _logger = logger;
        }
        public async Task<MonthlyBalance> GetPlatformBalance(int month, int year)
        {
            using (var ctx = new DataContext())
            {
                var result = await ctx.MonthlyBalances.FirstOrDefaultAsync(t => t.Month == month && t.Year == year);

                return result;
            }
        }

        public async Task<decimal> GetTotalMoneyFromSubscriptions()
        {
            using(var ctx = new DataContext())
            {
                var usersSubc = await ctx.UserBillingPlans.Where(t => t.ActivationDate.HasValue && t.StripeSubscriptionId != null).ToListAsync();

                var invSvc = new InvoiceService();
                decimal total = 0;
                
                foreach (var sub in usersSubc)
                {
                    var invoices = await invSvc.ListAsync(new InvoiceListOptions() { Subscription = sub.StripeSubscriptionId, Status = "paid" });

                    if (invoices.Any())
                    {
                        var summed = invoices.Sum(x => x.AmountPaid);

                        total += Math.Round((decimal)summed / 100m, 2);
                    }
                }

                return total;
            }
        }

        public async Task<BalanceKPIResponse> BalanceKPITotal()
        {
            BalanceKPIResponse resp = new BalanceKPIResponse();

            using(var ctx = new DataContext())
            {
                var balances = await ctx.MonthlyBalances.ToListAsync();

                resp.StripeFees = Math.Round(balances.Sum(f => f.Fees),2);
                // resp.TotalIncome = Math.Round(balances.Sum(f => f.BalanceFull) / 100m, 2);
                resp.TotalIncome = await GetTotalMoneyFromSubscriptions();
                resp.MoneyForCoaches = Math.Round(balances.Sum(f => f.BalancaeForWithdrawals) / 100m, 2);
                resp.Currency = balances.FirstOrDefault()?.Currency;
                resp.MoneyEarnedByCoaches = 0;
                resp.MoneyForAffiliation = 0;
                

                var affiliates = await ctx.AffiliatePayments.ToListAsync();

                resp.MoneyForAffiliation = Math.Round(affiliates.Sum(x => x.PaymentValue), 2);

                resp.AffiliationCurrency = affiliates.FirstOrDefault()?.PaymentCurrency;

                var coachesBalances = await ctx.CoachDailyBalance.Where(t=>t.Calculated).ToListAsync();

   
                resp.MoneyEarnedByCoaches = coachesBalances.Sum(x => x.BalanceValue);

                resp.MoneyEarnedByCoaches = Math.Round(resp.MoneyEarnedByCoaches / 100m, 2);

            }

            return resp;
        }

        public async Task<List<MonthBalanceKPIResponse>> GetBalanceForChartByYear(int year)
        {
            List<MonthBalanceKPIResponse> data = new List<MonthBalanceKPIResponse>();

            var dtStart = new DateTime(year, 1, 1);

            for(int i= 0;i<12;i++)
            {
                var dtTemp = dtStart.AddMonths(i);
                var balance = await BalanceKPIForMonth(dtTemp.Month, dtTemp.Year);

                if (balance == null)
                {
                    var resp = new MonthBalanceKPIResponse();
                    resp.Month = dtTemp;
                    resp.Currency = "eur";
                    resp.AffiliationCurrency = "eur";

                    data.Add(resp);

                }
                else
                {
                    data.Add(balance);
                }
            }

            return data;
        }

        public async Task<List<MonthBalanceKPIResponse>> GetBalanceForChartByPeriod(DateTime monthStart, DateTime monthEnd)
        {
            List<MonthBalanceKPIResponse> data = new List<MonthBalanceKPIResponse>();

            var dtStart = new DateTime(monthStart.Year, monthStart.Month, 1);
            var dtEnd = new DateTime(monthEnd.Year, monthEnd.Month, 1);

            if(dtStart> dtEnd)
            {
                throw new CoachOnlineException("Incorrect date range", CoachOnlineExceptionState.DataNotValid);
            }
            int i = 0;
            while(dtStart.AddMonths(i) <= dtEnd)
            {
                var dtTemp = dtStart.AddMonths(i);
                var balance = await BalanceKPIForMonth(dtTemp.Month, dtTemp.Year);

                if (balance == null)
                {
                    var resp = new MonthBalanceKPIResponse();
                    resp.Month = dtTemp;
                    resp.Currency = "eur";
                    resp.AffiliationCurrency = "eur";

                    data.Add(resp);

                }
                else
                {
                    data.Add(balance);
                }
                i++;
            }

            return data;
        }

        public async Task<MonthBalanceKPIResponse> BalanceKPIForMonth(int month, int year)
        {
            using (var ctx = new DataContext())
            {
                var balance = await ctx.MonthlyBalances.Where(t => t.Year == year && t.Month == month).FirstOrDefaultAsync();

                if (balance != null)
                {
                    MonthBalanceKPIResponse mthBalance = new MonthBalanceKPIResponse();
                    mthBalance.Month = new DateTime(year, month, 1);
                    mthBalance.Currency = balance.Currency;
                    mthBalance.StripeFees = balance.Fees;
                    mthBalance.TotalIncome = balance.BalanceFull / 100m;
                    mthBalance.MoneyForCoaches = balance.BalancaeForWithdrawals / 100m;
                    mthBalance.MoneyEarnedByCoaches = 0;
                    mthBalance.MoneyForAffiliation = 0;


                    var affiliates = await ctx.AffiliatePayments.Where(t => t.PaymentCreationDate >= mthBalance.Month && t.PaymentCreationDate < mthBalance.Month.AddMonths(1)).ToListAsync();

                    mthBalance.MoneyForAffiliation = Math.Round(affiliates.Sum(x => x.PaymentValue),2);

                    mthBalance.AffiliationCurrency = affiliates.FirstOrDefault()?.PaymentCurrency;

                    var coachesMoney = await ctx.CoachMonthlyBalance.Where(t => t.MonthlyBalanceId == balance.Id).Include(x => x.DayBalances).ToListAsync();

                    foreach(var c in coachesMoney)
                    {
                      
                        mthBalance.MoneyEarnedByCoaches += c.DayBalances.Where(t=>t.Calculated).Sum(x => x.BalanceValue);
                    }

                    mthBalance.MoneyEarnedByCoaches = Math.Round(mthBalance.MoneyEarnedByCoaches / 100m, 2);

                    return mthBalance;
                }
               
                return null;

            }
        }

        public async Task PrepareCoachBalancesForMonth(int month, int year)
        {
            var period = new DateTime(year, month, 1);
            var balancePeriod = period.AddDays(-1);

            var daysInMonth = DateTime.DaysInMonth(year, month);

            using (var ctx = new DataContext())
            {
                var mBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(t => t.Month == balancePeriod.Month && t.Year == balancePeriod.Year);
                if (mBalance == null)
                {
                    return;
                }
                // && t.WithdrawalsEnabled && t.PaymentsEnabled && t.Status == UserAccountStatus.CONFIRMED
                var coaches = await ctx.users.Where(t => t.UserRole == UserRoleType.COACH && t.Status != UserAccountStatus.DELETED).ToListAsync();

                foreach (var coach in coaches)
                {
                    var coachMthBalance = await ctx.CoachMonthlyBalance.Include(t => t.DayBalances).FirstOrDefaultAsync(t => t.CoachId == coach.Id && t.MonthlyBalanceId == mBalance.Id);
                    if (coachMthBalance == null)
                    {
                        var entity = new CoachBalanceMonth();
                        entity.CoachId = coach.Id;
                        entity.Month = month;
                        entity.Year = year;
                        entity.MonthlyBalanceId = mBalance.Id;
                        entity.TotalMonthBalance = 0;
                        entity.DayBalances = new List<CoachBalanceDay>();
                        int dayNo = 1;
                        while (dayNo <= daysInMonth)
                        {
                            entity.DayBalances.Add(new CoachBalanceDay()
                            {
                                BalanceDay = new DateTime(year, month, dayNo),
                                BalanceValue = 0,
                                Transferred = false
                            });
                            dayNo++;
                        }

                        ctx.CoachMonthlyBalance.Add(entity);
                        await ctx.SaveChangesAsync();
                    }
                }

            }
        }


        public async Task CalclateCoachesBalanceForDay(DateTime day)
        {
            using (var ctx = new DataContext())
            {
                var balances = await ctx.CoachMonthlyBalance.Where(t => t.Month == day.Month && t.Year == day.Year).Include(x => x.MonthlyBalance).ToListAsync();
                if (!balances.Any())
                {
                    return;
                }

                decimal maxPayment = MaxPaymentPerDay(DateTime.DaysInMonth(day.Year, day.Month), balances.First().MonthlyBalance);
                Console.WriteLine("Maax payment is " + maxPayment);
                decimal totalWatchedTime = await GetWatchedEpisodesTimeAllByDay(day.Date);
                Console.WriteLine("Total watched time " + totalWatchedTime);
                var values = await CalculatePayment(maxPayment, day, balances, totalWatchedTime);

                var grouppedByCoach = balances.GroupBy(t => t.CoachId).ToList();

                foreach (var b in balances)
                {
                    //!x.Calculated
                    var dailyB = await ctx.CoachDailyBalance.FirstOrDefaultAsync(x => x.BalanceDay == day && x.CoachBalanceMonthId == b.Id && !x.Transferred);
                    if (dailyB != null)
                    {
                        var payVal = values.First(t => t.Key == b.CoachId);
                        dailyB.BalanceValue = payVal.Value.Payment;
                        dailyB.TotalEpisodesWatchTime = payVal.Value.TotalWatchedTime;
                        dailyB.Calculated = true;
                        await ctx.SaveChangesAsync();
                    }

                    var total = await ctx.CoachMonthlyBalance.Where(x => x.Id == b.Id).Include(x => x.DayBalances).FirstOrDefaultAsync();
                    total.TotalMonthBalance = total.DayBalances.Sum(t => t.BalanceValue);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public class CoachBalanceInfo
        {
            public int CoachId { get; set; }
            public decimal Payment { get; set; }
            public decimal TotalWatchedTime { get; set; }
        }

        private async Task<Dictionary<int, CoachBalanceInfo>> CalculatePayment(decimal maxPay, DateTime day, List<CoachBalanceMonth> balances, decimal totalWatchedTime)
        {
            var dict = new Dictionary<int, CoachBalanceInfo>();

            var grouppedByCoach = balances.GroupBy(t => t.CoachId).ToList();
            if (grouppedByCoach.Count > 0)
            {

                decimal xVal = Math.Round(maxPay / (decimal)grouppedByCoach.Count);
                foreach (var b in grouppedByCoach)
                {
                    var balance = new CoachBalanceInfo();
                    balance.CoachId = b.Key;

                    var coachTotalWatchedTime = await GetWatchedEpisodesTimeByCoachAndDay(b.Key, day.Date);
                    // Console.WriteLine($"Coach {b.Key} total watch time is {coachTotalWatchedTime}");
                    balance.TotalWatchedTime = coachTotalWatchedTime;
                    if (totalWatchedTime > 0)
                    {
                        balance.Payment = Math.Round((coachTotalWatchedTime / totalWatchedTime) * maxPay, 2);
                    }
                    else
                    {
                        balance.Payment = 0;
                    }
                    dict.Add(b.Key, balance);
                }
            }

            return dict;
        }


        private async Task<decimal> GetWatchedEpisodesTimeAllByDay(DateTime day)
        {

            using (var ctx = new DataContext())
            {
                var episodes = await ctx.UserWatchedEpisodes.Where(t => t.Day <= day.Date).ToListAsync();


                var byEpisode = episodes.GroupBy(e => e.EpisodeId);
                decimal totalWatchTime = 0;
                foreach (var epGrp in byEpisode)
                {
                    var byUser = epGrp.GroupBy(u => u.UserId);

                    foreach (var usrGrp in byUser)
                    {
                        var todayWatchTime = usrGrp.FirstOrDefault(x => x.Day == day.Date);
                        if (todayWatchTime != null)
                        {
                            var previousDaysWatchTime = usrGrp.Where(x => x.Day != day.Date).Sum(t => t.EpisodeWatchedTime);

                            if (todayWatchTime.EpisodeWatchedTime - previousDaysWatchTime > 0)
                            {
                                totalWatchTime += todayWatchTime.EpisodeWatchedTime - previousDaysWatchTime;
                            }

                        }
                    }
                }
                return totalWatchTime;
            }
        }

        private async Task<decimal> GetWatchedEpisodesTimeByCoachAndDay(int coachId, DateTime day)
        {
            using (var ctx = new DataContext())
            {
                var ownedCourses = await ctx.users.Where(t => t.Id == coachId).Include(c => c.OwnedCourses).ThenInclude(ep => ep.Episodes).FirstOrDefaultAsync();

                List<Episode> episodes = new List<Episode>();
                foreach (var course in ownedCourses.OwnedCourses)
                {
                    episodes.AddRange(course.Episodes);
                }


                if (episodes.Count > 0)
                {
                    var epsIds = episodes.Select(t => t.Id).ToList();
                    var getEpisodesWatchedByUsers = ctx.UserWatchedEpisodes.Where(t => t.Day <= day.Date && epsIds.Contains(t.EpisodeId)).ToList();
                    var byEpisode = getEpisodesWatchedByUsers.GroupBy(e => e.EpisodeId).ToList();
                    decimal totalWatchTime = 0;
                    foreach (var epGrp in byEpisode)
                    {
                        var byUser = epGrp.GroupBy(u => u.UserId);

                        foreach (var usrGrp in byUser)
                        {
                            var todayWatchTime = usrGrp.FirstOrDefault(x => x.Day == day.Date);
                            if (todayWatchTime != null)
                            {
                                var previousDaysWatchTime = usrGrp.Where(x => x.Day != day.Date).Sum(t => t.EpisodeWatchedTime);

                                if (todayWatchTime.EpisodeWatchedTime - previousDaysWatchTime > 0)
                                {
                                    totalWatchTime += todayWatchTime.EpisodeWatchedTime - previousDaysWatchTime;
                                }

                            }
                        }
                    }
                    return totalWatchTime;
                }


                return 0;

            }
        }


        private decimal MaxPaymentPerDay(int daysInMonth, MonthlyBalance balance)
        {
            return balance.BalancaeForWithdrawals / daysInMonth;
        }

        public class PlatformBalanceData
        {
            public int Month { get; set; }
            public int Year { get; set; }
            public decimal Balance { get; set; }
            public decimal Fees { get; set; }
            public string Currency { get; set; }
        }

        public async Task<PlatformBalanceData> CalculatePlatformBalanceForPeriod(int month, int year)
        {
            try
            {
                PlatformBalanceData resp = new PlatformBalanceData();
                resp.Month = month;
                resp.Year = year;
                List<Stripe.Subscription> subsList = new List<Subscription>();
                var dt = new DateTime(year, month, 1);
                var dtNextMth = dt.AddMonths(1);
                var subListOptions = new SubscriptionListOptions() { Status = "all", Limit = 100, Created = new DateRangeOptions() { LessThan = dtNextMth.ToUniversalTime() } };

                var subSvc = new Stripe.SubscriptionService();
                var allSubs = await subSvc.ListAsync(subListOptions);
                subsList.AddRange(allSubs.Data);

                int currentDataCount = allSubs.Data.Count;
                while (currentDataCount == 100)
                {
                    var lastSub = subsList.Last();
                    var subListOptionsOth = new SubscriptionListOptions() { Status = "all", StartingAfter = $"{lastSub.Id}", Limit = 100, Created = new DateRangeOptions() { LessThan = dtNextMth.ToUniversalTime() } };
                    var otherSubs = await subSvc.ListAsync(subListOptionsOth);
                    currentDataCount = otherSubs.Data.Count;
                    if (currentDataCount > 0)
                    {
                        subsList.AddRange(otherSubs.Data);
                    }
                }

                decimal totalAmount = 0;
                string amountCurrency = "";
                decimal totalFees = 0;

                subsList = subsList.Where(t => t.Status == "active" || t.Status == "canceled").ToList();

                foreach (var sub in subsList)
                {
                    if (sub.Items.Any() && sub.Items.Data.Any() && sub.Items.Data.Count > 0)
                    {
                        var item = sub.Items.Data[0];

                        var amount = item.Plan.Amount;
                        var interval = item.Plan.Interval;
                        var intCount = (int)item.Plan.IntervalCount;

                        var invSvc = new Stripe.InvoiceService();
                        var invoices = await invSvc.ListAsync(new InvoiceListOptions() { Subscription = sub.Id });

                        foreach (var invoice in invoices.Data)
                        {
                            if (invoice.Status == "paid")
                            {
                                if (string.IsNullOrEmpty(amountCurrency))
                                {
                                    amountCurrency = invoice.Currency;
                                }

                                if (!string.IsNullOrEmpty(amountCurrency) && amountCurrency != invoice.Currency)
                                {
                                    _logger.LogInformation("Wrong Currency for amount");
                                }
                                else
                                {
                                    amountCurrency = invoice.Currency;
                                }
                                //for month periods
                                if (invoice.Created.ToUniversalTime() >= dt.ToUniversalTime() && invoice.Created.ToUniversalTime() < dtNextMth.ToUniversalTime() && interval != "year" && intCount == 1)
                                {
                                    totalAmount += Math.Round((decimal)invoice.AmountPaid / 100, 2);
                                }
                                else if (interval == "year" && intCount == 1 && invoice.Created.ToUniversalTime() >= dtNextMth.AddYears(-1).ToUniversalTime() && invoice.Created.ToUniversalTime() < dtNextMth.ToUniversalTime())//for year
                                {

                                    totalAmount += Math.Round((decimal)invoice.AmountPaid / 12 / 100, 2);

                                }
                                else
                                {
                                    if (intCount != 1)
                                    {
                                        _logger.LogInformation($"Subscription interval count bigger than 1 period => {intCount}");
                                    }
                                }

                                if (invoice.Created.ToUniversalTime() >= dt.ToUniversalTime() && invoice.Created.ToUniversalTime() < dtNextMth.ToUniversalTime())
                                {
                                    var payIntent = invoice.PaymentIntentId;
                                    if (!string.IsNullOrEmpty(payIntent))
                                    {
                                        var paySvc = new Stripe.PaymentIntentService();
                                        var payment = await paySvc.GetAsync(payIntent);

                                        if (payment.Charges.Data.Any())
                                        {
                                            //stripe fee
                                            var balanceSvc = new Stripe.BalanceTransactionService();
                                            var balance = await balanceSvc.GetAsync(payment.Charges.Data[0].BalanceTransactionId);



                                            if (balance.ExchangeRate.HasValue)
                                            {
                                                totalFees += Math.Round((decimal)balance.Fee / 100 / balance.ExchangeRate.Value, 2);
                                            }
                                            else
                                            {
                                                totalFees += Math.Round((decimal)balance.Fee / 100);
                                            }
                                        }
                                    }
                                }
                            }

                        }

                    }

                }

                resp.Balance = totalAmount;
                resp.Fees = totalFees;
                resp.Currency = amountCurrency;
                return resp;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task UpdatePlatformBalance(int month, int year)
        {
            var today = DateTime.Today;
            if(today.Month == month && today.Year == year)
            {
                //we accumulate balance for previous month
                return;
            }
            using(var ctx = new DataContext())
            {
                var mBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(t => t.Month == month && t.Year == year);
                if(mBalance == null)
                {
                    var balance = await CalculatePlatformBalanceForPeriod(month, year);
                  // var service = new Stripe.BalanceService();
                  //var balance = await service.GetAsync();
                    if (balance != null)
                    {
                       // var b = balance.Available[0];

                        var entity = new MonthlyBalance()
                        {
                            BalanceFull = (long)balance.Balance*100,
                            BalancaeForWithdrawals = Math.Round((balance.Balance * 100 * 0.2m), 2),
                            Month = month,
                            Year = year,
                            Fees = balance.Fees,
                            Currency = balance.Currency,
                            CalculationDate = DateTime.Now
                        };

                        ctx.MonthlyBalances.Add(entity);
                        await ctx.SaveChangesAsync();
                    }
                }
                //else already counted leave it
            }
        }
    }
}
