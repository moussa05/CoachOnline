using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using CoachOnline.Model.ApiResponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BalanceController : ControllerBase
    {

        private readonly ILogger<BalanceController> _logger;
        private readonly ICounter _counterSvc;
        private readonly IUser _userSvc;
        private readonly IBalance _balanceSvc;

        public BalanceController(ILogger<BalanceController> logger, ICounter counterSvc, IUser userSvc, IBalance balanceSvc)
        {
            _logger = logger;
            _counterSvc = counterSvc;
            _userSvc = userSvc;
            _balanceSvc = balanceSvc;
        }

       //// [Authorize]
       // [HttpGet("{year}/{month}")]
       // public async Task<IActionResult> TestCalculate(int year, int month)
       // {
       //     try
       //     {
       //         //var role = User.GetUserRole();

       //         //if(role!= UserRoleType.ADMIN.ToString())
       //         //{
       //         //    throw new CoachOnlineException("Unauthorized", CoachOnlineExceptionState.NotAuthorized);
       //         //}
       //         var resp = await _balanceSvc.CalculatePlatformBalanceForPeriod(month, year);

       //         return new OkObjectResult(resp);
       //         return Ok();
       //     }
       //     catch (CoachOnlineException e)
       //     {
       //         //_logger.LogError(e.Message);

       //         return new CoachOnlineActionResult(e);
       //     }
       //     catch (Exception e)
       //     {

       //        // _logger.LogError(e.Message);
       //         return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
       //     }
       // }

        [Authorize]
        [HttpGet("/api/[controller]/total")]
        public async Task<IActionResult> GetTotalBalanceKPI()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _balanceSvc.BalanceKPITotal();

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [Authorize]
        [HttpGet("/api/[controller]/total/{year}/{month}")]
        public async Task<IActionResult> GetMonthBalanceKPI(int year, int month)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _balanceSvc.BalanceKPIForMonth(month, year);

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {

                 _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPost("/api/[controller]/byperiod")]
        public async Task<IActionResult> GetYearBalanceForChart([FromBody] TimeRangeRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }

                
                if(!rqs.Start.HasValue || !rqs.End.HasValue)
                {
                    throw new CoachOnlineException("Date range not provided", CoachOnlineExceptionState.DataNotValid);
                }

                var resp = await _balanceSvc.GetBalanceForChartByPeriod(rqs.Start.Value, rqs.End.Value);

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {

                 _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpGet("/api/[controller]/chart/{year}")]
        public async Task<IActionResult> GetYearBalanceForChart(int year)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }



                var resp = await _balanceSvc.GetBalanceForChartByYear(year);

                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {

                 _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetPlatformBalance([FromBody] PlatformBalanceRqs rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AdminAuthToken);

                user.CheckExist("User");
    
                DateTime dt;
                if (!rqs.Month.HasValue || !rqs.Year.HasValue)
                {
                    dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                }
                else
                {
                    dt = new DateTime(rqs.Year.Value, rqs.Month.Value, 1);
                }


                var dtForBalance = dt.AddMonths(-1);

                var balance = new PlatformBalanceResponse();
                
                
                using(var ctx = new DataContext())
                {
                    var dbBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month);

                    if(dbBalance!= null)
                    {
                        var data = await ctx.CoachMonthlyBalance.Where(t => t.MonthlyBalanceId == dbBalance.Id).Include(x => x.DayBalances).ToListAsync();
                        balance.Currency = dbBalance.Currency;
                        balance.Month = dt;
                        balance.TotalBalance = Math.Round((decimal)dbBalance.BalanceFull/100,2);
                        balance.BalanceForCoaches = Math.Round(dbBalance.BalancaeForWithdrawals/100,2);
                        balance.TotalWatchedMinutes = Math.Round(data.Sum(t => t.DayBalances.Sum(x => x.TotalEpisodesWatchTime))/60,2);
                    }
                    
                }
                return new OkObjectResult(balance);

            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> GetCoachBalance([FromBody] CoachBalanceRqs rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AdminAuthToken);

                user.CheckExist("User");

                DateTime? dt = null;
                if (!rqs.Month.HasValue || !rqs.Year.HasValue)
                {
                    dt = null;
                }
                else
                {
                    dt = new DateTime(rqs.Year.Value, rqs.Month.Value, 1);
                }

                var coachBalanceResponse = new CoachBalanceResponse();
                coachBalanceResponse.Month = dt.HasValue ? dt.Value : new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                coachBalanceResponse.DayBalances = new List<CoachDayBalanceResponse>();
                coachBalanceResponse.TotalBalance = 0;
                coachBalanceResponse.TotalWatchedMinutes = 0;
                coachBalanceResponse.Currency = "eur";


                using (var ctx = new DataContext())
                {
                    if (dt.HasValue)
                    {
                        var dtForBalance = dt.Value.AddMonths(-1);
                        var dbBalance = await ctx.MonthlyBalances.FirstOrDefaultAsync(x => x.Year == dtForBalance.Year && x.Month == dtForBalance.Month);
                        if (dbBalance != null)
                        {
                            var monthBalance = await ctx.CoachMonthlyBalance.Where(t => t.CoachId == rqs.CoachId && t.MonthlyBalanceId == dbBalance.Id).Include(d => d.DayBalances).FirstOrDefaultAsync();

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
                    else
                    {
                        var coachBalances = await ctx.CoachMonthlyBalance.Where(x => x.CoachId == rqs.CoachId).Include(m => m.MonthlyBalance).Include(d => d.DayBalances).ToListAsync();

                        foreach (var balance in coachBalances)
                        {
                            foreach (var day in balance.DayBalances)
                            {
                                coachBalanceResponse.DayBalances.Add(new CoachDayBalanceResponse()
                                {
                                    Day = day.BalanceDay,
                                    DayBalance = Math.Round(day.BalanceValue / 100, 2),
                                    TotalWatchedMinutes = Math.Round(day.TotalEpisodesWatchTime / 60, 2)
                                });
                            }

                            coachBalanceResponse.TotalBalance = coachBalanceResponse.DayBalances.Sum(t => t.DayBalance);
                            coachBalanceResponse.TotalWatchedMinutes = coachBalanceResponse.DayBalances.Sum(t => t.TotalWatchedMinutes);
                            coachBalanceResponse.Currency = balance.MonthlyBalance.Currency;
                        }
                    }
                }



                return new OkObjectResult(coachBalanceResponse);

            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {

                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}
