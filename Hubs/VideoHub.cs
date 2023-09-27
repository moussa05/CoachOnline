using CoachOnline.Helpers;
using CoachOnline.Hubs.Model;
using CoachOnline.Implementation;
using CoachOnline.Interfaces;
using CoachOnline.Mongo;
using CoachOnline.Mongo.Models;
using CoachOnline.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Hubs
{
   // [Authorize]
    public class VideoHub:Hub
    {
        private readonly IHubUserInfoInMemory _usersInfoInMemory;
        private readonly IUser _userSvc;
        private MongoCtx _mongoCtx;


        public VideoHub(IHubUserInfoInMemory usersInfoInMemory,IUser userSvc, MongoCtx mongoCtx)
        {
            _usersInfoInMemory = usersInfoInMemory;
            _userSvc = userSvc;
            _mongoCtx = mongoCtx;
        }


        public async Task Stop(PlayerStatusInfo episode)
        {
            if(string.IsNullOrEmpty(episode.authToken))
            {
                return;
            }

            var user = await _userSvc.GetUserByTokenAsync(episode.authToken);
            user.CheckExist("User");

            //if(user.UserRole == CoachOnline.Model.UserRoleType.COACH)
            //{
            //    return;
            //}

            var userId = user.Id;

  
            if (userId == 0 || episode.episodeId == 0)
            {
                return;
            }

            await SaveVideoStatus(episode);
            _usersInfoInMemory.Remove(userId, episode.episodeId);

            await Clients.Client(Context.ConnectionId).SendAsync("VideoStopped");

        }

   
        public async Task Play(PlayerStatusInfo episode)
        {
            //Console.WriteLine($"Play received, episodeid {episode.episodeId}, user token: {episode.authToken}");

            if (string.IsNullOrEmpty(episode.authToken))
            {
                return;
            }

            var user = await _userSvc.GetUserByTokenAsync(episode.authToken);
            user.CheckExist("User");

            var userId = user.Id;

     
            if (userId == 0 || episode.episodeId == 0)
            {
                return;
            }

           // Console.WriteLine($"User {userId} played video");

            if (!_usersInfoInMemory.AddUpdate(userId, Context.ConnectionId, episode.episodeId))
            {
                //item disnt exist before
            }
           
            var data = _usersInfoInMemory.GetAllByUserId(userId);

            var otherEpisodes = data.Where(t => t.EpisodeId != episode.episodeId).ToList();
            if(otherEpisodes !=null)
            {
                foreach(var ep in otherEpisodes)
                {
                    Console.WriteLine($"sending stop video for another episode: {ep.EpisodeId}. ConnectionId: {ep.ConnectionId}");
                    await Clients.Client(ep.ConnectionId).SendAsync("StopVideo",episode.episodeId);
                }
            }


            await ManageDb(episode, userId);

            await Clients.Client(Context.ConnectionId).SendAsync(
                "VideoStarted",$""
                );
        }

        public async Task SaveVideoStatus(PlayerStatusInfo episode)
        {

            if (string.IsNullOrEmpty(episode.authToken))
            {
                return;
            }

            var user = await _userSvc.GetUserByTokenAsync(episode.authToken);
            user.CheckExist("User");

            //if (user.UserRole == CoachOnline.Model.UserRoleType.COACH)
            //{
            //    return;
            //}

            var userId = user.Id;
     
            if (userId == 0 || episode.episodeId == 0)
            {
                return;
            }
            await ManageDb(episode, userId);
            //Console.WriteLine($"Saving status=> current: {episode.currentSecond}s, duration: {episode.duration}, rate: {episode.rate}");

        }

        private async Task<bool> IsEpisodeAPromo(int episodeId)
        {
            using(var ctx = new DataContext())
            {
                var ep = await ctx.Episodes.FirstOrDefaultAsync(t => t.Id == episodeId);
                if(ep!=null)
                {
                    var result = ep.IsPromo.HasValue && ep.IsPromo.Value;

                    return result;
                }

                return false;
            }
        }

        private async Task ManageDb(PlayerStatusInfo episode, int userId)
        {
            var data = await _mongoCtx.UserEpisodes.FindByUserIdAndEpisodeId(episode.episodeId, userId);
            if (!data.Any())
            {
                await _mongoCtx.UserEpisodes.InsertAsync(new UserEpisode
                {
                    EpisodeId = episode.episodeId,
                    UserId = userId,
                    Duration = episode.duration,
                    Timestamps = new List<EpisodeTimestamp>()
                    {
                        new EpisodeTimestamp{Value = episode.currentSecond, UpdateTime = DateTime.Now, Rate = episode.rate}
                    }
                });
            }
            else
            {
                var itm = data.First();
                

                if(itm.Timestamps.Any(x=>x.Value == episode.currentSecond && x.Rate == episode.rate))
                {
                    return;
                }

                itm.Duration = episode.duration;
                if (itm.Timestamps == null)
                {
                    itm.Timestamps = new List<EpisodeTimestamp>();
                }
                itm.Timestamps.Add(new EpisodeTimestamp { Value = episode.currentSecond, UpdateTime = DateTime.Now, Rate = episode.rate });
                await _mongoCtx.UserEpisodes.UpdateAsync(itm);
            }
        }

    }
}
