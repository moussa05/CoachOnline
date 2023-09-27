using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Mongo;
using CoachOnline.Mongo.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CoachOnline.Hubs
{
    public class VideoStatusChecker: BackgroundService
    {
        private readonly IHubContext<VideoHub> _hubContext;
        private readonly ILogger<VideoStatusChecker> _logger;
        private readonly IHubUserInfoInMemory _usersInfoInMemory;
        private readonly IHubContext<ActiveUsersHub> _hubActiveUsers;

        public VideoStatusChecker(IHubContext<VideoHub> hubContext, ILogger<VideoStatusChecker> logger, IHubUserInfoInMemory usersInfoInMemory, IHubContext<ActiveUsersHub> hubActiveUsers)
        {
            _hubContext = hubContext;
            _logger = logger;
            _usersInfoInMemory = usersInfoInMemory;
            _hubActiveUsers = hubActiveUsers;
        }

        //static object x = new object();
        //private readonly SemaphoreSlim mySemaphoreSlim = new SemaphoreSlim(1, 1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            decimal timer = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var users = _usersInfoInMemory.GetAllUsers();
                    if (users != null)
                    {
                        foreach (var u in users)
                        {
                            //Console.WriteLine($"Checking status of :{u.UserId}, connectionId: {u.ConnectionId}, episode: {u.EpisodeId}");
                            await _hubContext.Clients.Client(u.ConnectionId).SendAsync("CheckVideoStatus");
                        }
                    }

                    //every 30 seconds
                    //if(timer == 30)
                    //{
                    //    timer = 0;
                    //    await _hubActiveUsers.Clients.All.SendAsync("CheckUserLocalization");
                    //}

                    await Task.Delay(5000);
                    //timer += 5;
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
