using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.PayPalIntegration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoachOnline.Workers
{
    public class BalanceWorkerSvc: BackgroundService
    {
        private readonly ILogger<BalanceWorkerSvc> _logger;
        private readonly IBalance _balanceSvc;
        private readonly ICounter _counterSvc;
        private readonly IPayPal _payPalSvc;
        public BalanceWorkerSvc(ILogger<BalanceWorkerSvc> logger, IBalance balanceSvc, ICounter counterSvc, IPayPal payPal)
        {
            _logger = logger;
            _balanceSvc = balanceSvc;
            _counterSvc = counterSvc;
            _payPalSvc = payPal;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var dt = DateTime.Today;
                    dt = new DateTime(dt.Year, dt.Month, 1);
                    dt = dt.AddMonths(-1);
                    await _counterSvc.CountAllEpisodes();
                    await _balanceSvc.UpdatePlatformBalance(dt.Month, dt.Year);
                    await _balanceSvc.PrepareCoachBalancesForMonth(DateTime.Today.Month, DateTime.Today.Year);
                    Console.WriteLine("Balance prepared");
                    await _balanceSvc.CalclateCoachesBalanceForDay(DateTime.Today.AddDays(-1));
                    await _counterSvc.SuggestVideos(DateTime.Today, 1);

                    await _payPalSvc.CheckPaymentStatuses();
                    await Task.Delay(1000*60*60);
                }
                catch (CoachOnlineException ex)
                {
                    _logger.LogInformation(ex.Message, ex.ExceptionStatus);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }
    }
}
