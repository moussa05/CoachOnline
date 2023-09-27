using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Mongo;
using CoachOnline.Mongo.Models;
using CoachOnline.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Hubs
{
    public class ActiveUsersHub: Hub
    {
        private readonly IUser _userSvc;
        private readonly ILogger<ActiveUsersHub> _logger;
        private readonly ILibraryManagement _libMgmtSvc;
        private readonly IQuestionnaire _questionSvc;
        private MongoCtx _mongoCtx;
        public ActiveUsersHub(IUser userSvc, ILogger<ActiveUsersHub> logger, MongoCtx mongoCtx, ILibraryManagement libMgmtSvc, IQuestionnaire questionSvc)
        {
            _userSvc = userSvc;
            _logger = logger;
            _mongoCtx = mongoCtx;
            _libMgmtSvc = libMgmtSvc;
            _questionSvc = questionSvc;
        }

        public async Task UserConnected(UserActivityRqs rqs)
        {
            try
            {
                if (string.IsNullOrEmpty(rqs.AuthToken))
                {
                    _logger.LogInformation("ActiveUsersHub - User token is empty");
                    return;
                }
                var user = await _userSvc.GetUserByTokenAllowNullAsync(rqs.AuthToken);

                if (user != null)
                {

               

                    if (user.UserRole == CoachOnline.Model.UserRoleType.INSTITUTION_STUDENT && user.InstitutionId.HasValue)
                    {


                        int current = await _libMgmtSvc.GetCurrentConnections(user.InstitutionId.Value);
                        int limit = await _libMgmtSvc.GetConnectionsLimitForLibrary(user.InstitutionId.Value);

                        Console.WriteLine($"Current: {current}, limit: {limit}");
                        if (current >= limit)
                        {
                            Console.WriteLine("Limit reached");
                            await Clients.Client(Context.ConnectionId).SendAsync("LimitReached", "Limit for active connections has been reached");

                            await AddUserConnected(Context.ConnectionId, user.InstitutionId.Value, user.Id, false);
                        }
                        else
                        {
                            await AddUserConnected(Context.ConnectionId, user.InstitutionId.Value, user.Id, true);
                        }


                    }

                    if (rqs.CourseOpened)
                    {
                        await UserOpenedCourse(user, rqs.AuthToken, rqs.DeviceInfo, rqs.IpAddress, rqs.Localization, Context.ConnectionId);


                        var answer = await _questionSvc.UserHasAnswered(user.Id);
                        var form = await _questionSvc.GetForm();
                        if (!answer && form != null)
                        {
                            await Clients.Client(Context.ConnectionId).SendAsync("FillFormRequest", form);
                        }
                    }

                }
            }
            catch (CoachOnlineException ex)
            {
                _logger.LogInformation(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

        }

        //public async Task UserConnected(string authToken)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(authToken))
        //        {
        //            _logger.LogInformation("ActiveUsersHub - User token is empty");
        //            return;
        //        }
        //        var user = await _userSvc.GetUserByTokenAllowNullAsync(authToken);

        //        if (user != null)
        //        {

        //            if (user.UserRole == CoachOnline.Model.UserRoleType.INSTITUTION_STUDENT && user.InstitutionId.HasValue)
        //            {


        //                int current = await _libMgmtSvc.GetCurrentConnections(user.InstitutionId.Value);
        //                int limit = await _libMgmtSvc.GetConnectionsLimitForLibrary(user.InstitutionId.Value);

        //                Console.WriteLine($"Current: {current}, limit: {limit}");
        //                if (current >= limit)
        //                {
        //                    Console.WriteLine("Limit reached");
        //                    await Clients.Client(Context.ConnectionId).SendAsync("LimitReached", "Limit for active connections has been reached");

        //                    await AddUserConnected(Context.ConnectionId, user.InstitutionId.Value, user.Id, false);
        //                }
        //                else
        //                {
        //                    await AddUserConnected(Context.ConnectionId, user.InstitutionId.Value, user.Id, true);
        //                }
        //                //add to active connections



        //                // await Clients.All.SendAsync("ReceiveMessage", "added:)");
        //            }
        //            else
        //            {
        //                return;
        //            }
        //        }
        //    }
        //    catch (CoachOnlineException ex)
        //    {
        //        _logger.LogInformation(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //    }

        //}

        public async Task UserLocalization(UserLocalizationRqs rqs)
        {
            if(string.IsNullOrEmpty(rqs.AuthToken))
            {
                return;
            }
            using (var ctx = new DataContext())
            {
                var token = await ctx.userLogins.FirstOrDefaultAsync(x => x.AuthToken == rqs.AuthToken && !x.Disposed);

                if (token != null)
                {
                    Console.WriteLine("Token hub id: "+token.HubConnectionId);
                    Console.WriteLine("Context hub id: " + Context.ConnectionId);
                    if (!string.IsNullOrEmpty(rqs.UserUrl) && rqs.UserUrl.ToLower().Contains("/course?id"))
                    {
                        bool isAllowedToWatch = true;
                        if (token.LastActivityDate.HasValue && token.HubConnectionId != null)
                        {
                            if(token.HubConnectionId != Context.ConnectionId && token.IsAllowedToWatch)
                            {
                                //the same token but other connection
                                var activityResp = new UserActivityResp() { DeviceInfo = token.DeviceInfo, IpAddress = token.IpAddress, Localization = token.PlaceInfo, UserId = token.UserId };
                                await Clients.Client(token.HubConnectionId).SendAsync("ActiveOnAnotherDevice", activityResp);
                            }

                            var dt = DateTime.Now.AddMinutes(-30);
                           
                            var allUserTokensLastMinutes = await ctx.userLogins.Where(x => x.Id != token.Id && x.UserId == token.UserId && !x.Disposed && x.HubConnectionId != null && x.LastActivityDate.HasValue  && x.LastActivityDate.Value> dt).ToListAsync();
                            foreach (var otherToken in allUserTokensLastMinutes)
                            {
                                if (otherToken.IsAllowedToWatch)
                                {
                                    if(otherToken.LastActivityDate.Value >= token.LastActivityDate.Value)
                                    {
                                        var activityResp = new UserActivityResp() { DeviceInfo = otherToken.DeviceInfo, IpAddress = otherToken.IpAddress, Localization = otherToken.PlaceInfo, UserId = otherToken.UserId };
  
                                        await Clients.Client(token.HubConnectionId).SendAsync("ActiveOnAnotherDevice", activityResp);
                                        isAllowedToWatch = false;
                                    }
                                    else
                                    {
                                        var activityResp = new UserActivityResp() { DeviceInfo = token.DeviceInfo, IpAddress = token.IpAddress, Localization = token.PlaceInfo, UserId = token.UserId };
                                        await Clients.Client(otherToken.HubConnectionId).SendAsync("ActiveOnAnotherDevice", activityResp);
                                    }
                                }
                            }
                        }

                        token.LastActivityDate = DateTime.Now;
                        token.HubConnectionId = Context.ConnectionId;
                        token.IsAllowedToWatch = isAllowedToWatch;
                    }
                    else
                    {
                        //changed route so no info
                        token.IsAllowedToWatch = false;
                        token.HubConnectionId = Context.ConnectionId;

                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task UserLoggedOut()
        {
            await UpdateUserDisconnected(Context.ConnectionId);
            await LoggedOutChangeTokenInfo(Context.ConnectionId);
            await Clients.Client(Context.ConnectionId).SendAsync("LoggedOut", "User has logged out");
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await DisconnectedChangeTokenInfo(Context.ConnectionId);
            await UpdateUserDisconnected(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        private async Task AddUserConnected(string connectionId, int instituteId, int userId, bool allowed)
        {
            var users = await _mongoCtx.InstitureUsersCollection.FindByUserConnection(connectionId);

            if (users == null || users.Count==0 || users.All(t=>t.ConnectionEndTime.HasValue))
            {
                var conn = new InstituteUserConnection();
                conn.ConnectionId = connectionId;
                conn.ConnectionStartTime = DateTime.Now;
                conn.InstituteId = instituteId;
                conn.UserId = userId;
                conn.IsAllowedToView = allowed;
                await _mongoCtx.InstitureUsersCollection.InsertAsync(conn);
            }

        }

        private async Task UpdateUserDisconnected(string connectionId)
        {
            var users = await _mongoCtx.InstitureUsersCollection.FindByUserConnection(connectionId);

            if(users!= null)
            {
                foreach(var u in users)
                {
                    if (!u.ConnectionEndTime.HasValue)
                    {
                        u.ConnectionEndTime = DateTime.Now;
                        await _mongoCtx.InstitureUsersCollection.UpdateAsync(u);
                    }
                }
            }
        }

        private async Task UserOpenedCourse(User u, string authToken, string device, string ip, string localization, string hubConnectionId)
        {
            using (var ctx = new DataContext())
            {
                var token = await ctx.userLogins.FirstOrDefaultAsync(x => !x.Disposed && x.AuthToken == authToken && x.UserId == u.Id);
                if (token != null)
                {

                    var activityResp = new UserActivityResp() { DeviceInfo = device, IpAddress = ip, Localization = localization, UserId = u.Id };
                    if (token.HubConnectionId != null && token.HubConnectionId != hubConnectionId)
                    {
                        await Clients.Client(token.HubConnectionId).SendAsync("ActiveOnAnotherDevice", activityResp);
                    }
                    //assigning new connectionId to token
                    token.HubConnectionId = hubConnectionId;
                    token.DeviceInfo = device;
                    token.IpAddress = ip;
                    token.PlaceInfo = localization;
                    token.LastActivityDate = DateTime.Now;
                    token.IsAllowedToWatch = true;
                    await ctx.SaveChangesAsync();

                    var otherUserTokens = await ctx.userLogins.Where(x => !x.Disposed
                    && x.UserId == token.UserId
                    && x.Id != token.Id
                    && x.LastActivityDate.HasValue
                    && x.IsAllowedToWatch).ToListAsync();

                    foreach (var t in otherUserTokens)
                    {
                        t.IsAllowedToWatch = false;

                        if (!string.IsNullOrEmpty(t.HubConnectionId))
                        {
                            await Clients.Client(t.HubConnectionId).SendAsync("ActiveOnAnotherDevice", activityResp);
                        }
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        private async Task LoggedOutChangeTokenInfo(string hubConnectionId)
        {
            using(var ctx = new DataContext())
            {
                var data = await ctx.userLogins.Where(x => x.HubConnectionId != null && x.HubConnectionId == hubConnectionId).ToListAsync();

                foreach(var t in data)
                {
                    t.IsAllowedToWatch = false;                   
                }

                await ctx.SaveChangesAsync();
            }
        }

        private async Task DisconnectedChangeTokenInfo(string hubConnectionId)
        {
            using (var ctx = new DataContext())
            {
                var data = await ctx.userLogins.Where(x => x.HubConnectionId != null && x.HubConnectionId == hubConnectionId).ToListAsync();

                foreach (var t in data)
                {
                    t.IsAllowedToWatch = false;
                    t.HubConnectionId = null;
                }

                await ctx.SaveChangesAsync();
            }
        }

    }

    public class UserActivityRqs
    {
        public string AuthToken { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string Localization { get; set; }
        public bool CourseOpened { get; set; }
    }

    public class UserActivityResp
    {
        public int UserId { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public string Localization { get; set; }
    }

    public class UserLocalizationRqs
    {
        public string AuthToken { get; set; }
        public string UserUrl { get; set; }
    }
}
