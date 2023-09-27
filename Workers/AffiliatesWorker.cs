using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoachOnline.Workers
{
    public class AffiliatesWorker: BackgroundService
    {
        private readonly IAffiliate _affSvc;
        private readonly ILogger<AffiliatesWorker> _logger;
        public AffiliatesWorker(ILogger<AffiliatesWorker> logger, IAffiliate affSvc)
        {
            _logger = logger;
            _affSvc = affSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //wait 5 minutes for worker to start
                    await Task.Delay(1000 * 60 * 5, stoppingToken);
                    await _affSvc.CheckAffiliatePayments();
                    await _affSvc.CheckPaymentStatuses();
         
                    //wait one hour
                    await Task.Delay(1000 * 60 * 60 *1, stoppingToken);
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
