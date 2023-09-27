using CoachOnline.Implementation.Exceptions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Services
{
    public class ElasticReindexWorker : BackgroundService
    {
        private readonly ILogger<ElasticReindexWorker> _logger;
        private readonly ISearch _searchSvc;
        public ElasticReindexWorker(ILogger<ElasticReindexWorker> logger, ISearch searchSvc)
        {
            _logger = logger;
            _searchSvc = searchSvc;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _searchSvc.ReindexAll();
                    Console.WriteLine("Data reindexed");
                    await Task.Delay(TimeSpan.FromMinutes(20));
                   
                }
                catch (CoachOnlineException ex)
                {
                    _logger.LogInformation(ex.Message, ex.ExceptionStatus);
                    await Task.Delay(5000);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);

                    await Task.Delay(5000);
                }
            }
        }
    }
}
