using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.Coach
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CoachAccountController : ControllerBase
    {
        ILogger<CoachAccountController> _logger;
        private readonly ICoach _coachSvc;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CoachAccountController(ILogger<CoachAccountController> logger, ICoach coachSvc, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _coachSvc = coachSvc;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet]
        public async Task<IActionResult> GetBalance(string authToken)
        {
            try
            {
                var coach = await _coachSvc.GetCoachByTokenAsync(authToken);
                coach.CheckExist("Coach");
                var data = await _coachSvc.GetCurrentAmountToWidthraw(coach);
                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetParentCategoriesForSuggestion()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _coachSvc.GetParentCategoriesForSuggestion();

                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SuggestNewCategory([FromBody]SuggestCategoryRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var coach = await _coachSvc.GetCoachByIdAsync(userId.Value);
                coach.CheckExist("Coach");

                await _coachSvc.SuggestNewCategory(userId.Value, rqs.CategoryName, rqs.HasParent, rqs.ParentId, rqs.AdultOnly);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetMinutesSummarized([FromBody] CoachOwnBalanceRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var coach = await _coachSvc.GetCoachByIdAsync(userId.Value);
                coach.CheckExist("Coach");

                DateTime dt;
                DateTime previousMth;
                if (!rqs.Month.HasValue || !rqs.Year.HasValue)
                {
                    dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                else
                {
                    dt = new DateTime(rqs.Year.Value, rqs.Month.Value, 1);
                }

                previousMth = dt.AddMonths(-1);

                var coachBalanceResponse = new CoachSummarizedMinutesReponse();
                coachBalanceResponse.Month = dt;
                coachBalanceResponse.TotalWatchedMinutesCurrentMonth = 0;
                coachBalanceResponse.TotalWatchedMinutesPreviousMonth = 0;
                
             
                using (var ctx = new DataContext())
                {
                    //current month
                    var dtForBalance = dt.AddMonths(-1);
                    var dbBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month);
                    if (dbBalance != null)
                    {
                        var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id && t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                        if (monthBalance != null && monthBalance.DayBalances != null)
                        {
                            coachBalanceResponse.TotalWatchedMinutesCurrentMonth = Math.Round(monthBalance.DayBalances.Sum(t => t.TotalEpisodesWatchTime)/60,2);
                        }
                    }

                    //previous month
                    var dtForBalancePrevious = dtForBalance.AddMonths(-1);
                    var dbBalancePrevious = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalancePrevious.Year && x.Month == dtForBalancePrevious.Month);
                    if (dbBalancePrevious != null)
                    {
                        var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id && t.MonthlyBalanceId == dbBalancePrevious.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                        if (monthBalance != null && monthBalance.DayBalances != null)
                        {
                            coachBalanceResponse.TotalWatchedMinutesPreviousMonth = Math.Round(monthBalance.DayBalances.Sum(t => t.TotalEpisodesWatchTime) / 60, 2);
                        }
                    }
                }



                return new OkObjectResult(coachBalanceResponse);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetCurrentRanking([FromBody] CoachOwnBalanceRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _coachSvc.GetCurrentCoachRanking(userId.Value, rqs.Month, rqs.Year);



                return new OkObjectResult(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        //private async Task<DateTime?> GetJoinDateOfCoach(int coachId)
        //{
        //    DateTime? joinDate = null;
        //    using(var ctx = new DataContext())
        //    {
        //        var coachJoin = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coachId).OrderBy(t => t.Year).ThenBy(t => t.Month).Include(d => d.DayBalances).FirstOrDefaultAsync();
        //        if(coachJoin!= null && coachJoin.DayBalances != null)
        //        {
        //            joinDate = coachJoin.DayBalances.OrderBy(t=>t.BalanceDay).FirstOrDefault(t => t.Calculated)?.BalanceDay;
        //        }
        //    }

        //    return joinDate;
        //}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetBalanceSummarized([FromBody] CoachOwnBalanceRqs rqs)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var coach = await _coachSvc.GetCoachByIdAsync(userId.Value);
                coach.CheckExist("Coach");

                DateTime? dt = null;
                DateTime? previousMth;
                if (!rqs.Month.HasValue || !rqs.Year.HasValue)
                {
                    dt = null;
                    previousMth = null;
                }
                else
                {
                    dt = new DateTime(rqs.Year.Value, rqs.Month.Value, 1);
                    previousMth = dt.Value.AddMonths(-1);
                }



                var coachBalanceResponse = new CoachSummarizedBalanceReponse();
                coachBalanceResponse.Month = dt.HasValue ? dt.Value : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                coachBalanceResponse.TotalBalanceCurrentMonth = 0;
                coachBalanceResponse.TotalBalancePreviousMonth = 0;
                coachBalanceResponse.TotalBalanceToWithdrawCurrentMonth = 0;
                coachBalanceResponse.TotalBalanceToWithdrawPreviousMonth = 0;
                coachBalanceResponse.Currency = "eur";


                using (var ctx = new DataContext())
                {
                    if (!dt.HasValue)
                    {
                        var monthBalances = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id).Include(d => d.DayBalances).Include(x => x.MonthlyBalance).ToListAsync();

                        foreach (var balance in monthBalances)
                        {
                            coachBalanceResponse.Currency = balance.MonthlyBalance.Currency;
                            coachBalanceResponse.TotalBalanceCurrentMonth += balance.DayBalances.Sum(t => Math.Round(t.BalanceValue / 100, 2));
                            coachBalanceResponse.TotalBalanceToWithdrawCurrentMonth += balance.DayBalances.Where(t => t.Transferred == false).Sum(t => Math.Round(t.BalanceValue/100,2));
                        }
                    }
                    else
                    {
                        //given month
                        var dtForBalance = dt.Value.AddMonths(-1);
                        var dbBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month);
                        if (dbBalance != null)
                        {
                            coachBalanceResponse.Currency = dbBalance.Currency;
                            var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id && t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                            if (monthBalance != null && monthBalance.DayBalances != null)
                            {
                                coachBalanceResponse.TotalBalanceCurrentMonth = monthBalance.DayBalances.Sum(t => Math.Round(t.BalanceValue / 100, 2));
                                coachBalanceResponse.TotalBalanceToWithdrawCurrentMonth = monthBalance.DayBalances.Where(t => t.Transferred == false).Sum(t => Math.Round(t.BalanceValue / 100, 2));
                            }
                        }

                        //previous month to given
                        var dtForBalancePrevious = dtForBalance.AddMonths(-1);
                        var dbBalancePrevious = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalancePrevious.Year && x.Month == dtForBalancePrevious.Month);
                        if (dbBalancePrevious != null)
                        {
                            var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id && t.MonthlyBalanceId == dbBalancePrevious.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                            if (monthBalance != null && monthBalance.DayBalances != null)
                            {
                                coachBalanceResponse.TotalBalancePreviousMonth = monthBalance.DayBalances.Sum(t => Math.Round(t.BalanceValue/ 100, 2));
                                coachBalanceResponse.TotalBalanceToWithdrawPreviousMonth = monthBalance.DayBalances.Where(t => t.Transferred == false).Sum(t => Math.Round(t.BalanceValue / 100, 2));
                            }
                        }
                    }
                }



                return new OkObjectResult(coachBalanceResponse);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetChartData([FromBody]CoachOwnBalanceRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var coach = await _coachSvc.GetCoachByIdAsync(userId.Value);
                coach.CheckExist("Coach");

                DateTime dt;
                if (!rqs.Month.HasValue || !rqs.Year.HasValue)
                {
                    dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                else
                {
                    dt = new DateTime(rqs.Year.Value, rqs.Month.Value, 1);
                }

                var coachBalanceResponse = new CoachBalanceResponse();
                coachBalanceResponse.Month = dt;
                coachBalanceResponse.DayBalances = new List<CoachDayBalanceResponse>();

                var dtForBalance = dt.AddMonths(-1);
                using (var ctx = new DataContext())
                {
                    var dbBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month);
                    if (dbBalance != null)
                    {
                        var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == coach.Id && t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

                        if (monthBalance != null)
                        {
                            foreach (var day in monthBalance.DayBalances)
                            {
                                coachBalanceResponse.DayBalances.Add(new CoachDayBalanceResponse()
                                {
                                    Day = day.BalanceDay,
                                    DayBalance = Math.Round(day.BalanceValue / 100, 2),
                                    TotalWatchedMinutes = Math.Round(day.TotalEpisodesWatchTime / 60, 2)
                                });
                            }
                        }

                        coachBalanceResponse.TotalBalance = coachBalanceResponse.DayBalances.Sum(t => t.DayBalance);
                        coachBalanceResponse.TotalWatchedMinutes = coachBalanceResponse.DayBalances.Sum(t => t.TotalWatchedMinutes);
                        coachBalanceResponse.Currency = dbBalance.Currency;
                    }
                }



                return new OkObjectResult(coachBalanceResponse);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountData(string authToken)
        {
            try
            {
                var coach = await _coachSvc.GetCoachByTokenAsync(authToken);
                coach.CheckExist("Coach");
                var data = await _coachSvc.GetAccountData(coach);
                return new OkObjectResult(data);
            
            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> WidthrawPayment([FromBody]AuthTokenOnlyRequest rqs)
        {
            try
            {
                var coach = await _coachSvc.GetCoachByTokenAsync(rqs.AuthToken);
                coach.CheckExist("Coach");
                await _coachSvc.WithdrawPayment(coach);
                return Ok();
            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}
