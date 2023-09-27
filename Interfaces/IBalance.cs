using CoachOnline.Model;
using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.BalanceService;

namespace CoachOnline.Interfaces
{
    public interface IBalance
    {
        Task<MonthlyBalance> GetPlatformBalance(int month, int year);
        Task UpdatePlatformBalance(int month, int year);
        Task PrepareCoachBalancesForMonth(int month, int year);
        Task CalclateCoachesBalanceForDay(DateTime day);
        Task<PlatformBalanceData> CalculatePlatformBalanceForPeriod(int month, int year);
        Task<MonthBalanceKPIResponse> BalanceKPIForMonth(int month, int year);
        Task<BalanceKPIResponse> BalanceKPITotal();
        Task<List<MonthBalanceKPIResponse>> GetBalanceForChartByYear(int year);
        Task<List<MonthBalanceKPIResponse>> GetBalanceForChartByPeriod(DateTime monthStart, DateTime monthEnd);
    }
}
