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
    public class UserServicesWorker: BackgroundService
    {

        private readonly ILogger<UserServicesWorker> _logger;
        private readonly IUser _userSvc;
        public UserServicesWorker(ILogger<UserServicesWorker> logger, IUser userSvc)
        {
            _logger = logger;
            _userSvc = userSvc;

        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //wait 2 minutes
                    await Task.Delay(1000 * 60 * 2, stoppingToken);

                    await _userSvc.SendEndOfDiscoveryModeEmails();

                    //wait every 10 minutes
                    await Task.Delay(1000 * 60 * 10 * 1, stoppingToken);
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
