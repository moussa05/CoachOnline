using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoachOnline.Workers
{
    public class InstitutionWorker: BackgroundService
    {
        private readonly ILogger<InstitutionWorker> _logger;
        private readonly IB2BManagement _b2bSvc;
        public InstitutionWorker(ILogger<InstitutionWorker> logger, IB2BManagement b2bSvc)
        {
            _logger = logger;
            _b2bSvc = b2bSvc;

        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //wait 1 minute
                    await Task.Delay(1000 * 60 * 1, stoppingToken);

                    await _b2bSvc.CheckLibrariesSubscriptionStates();

                    await _b2bSvc.AutoRenewSubscriptions();

                    //wait every 5 minutes
                    await Task.Delay(1000 * 60 * 5 * 1, stoppingToken);
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
