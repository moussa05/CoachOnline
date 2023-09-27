using CoachOnline.Implementation;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Mongo;
using CoachOnline.Mongo.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class WatchTimeCounterService:ICounter
    {
        private MongoCtx _mongoCtx;


        public WatchTimeCounterService(MongoCtx mongoCtx)
        {
            _mongoCtx = mongoCtx;
        }



        public async Task CountAllEpisodes()
        {
            using(var ctx = new DataContext())
            {

                //count watched by students
                var users = await ctx.users.Where(t=>t.UserRole == UserRoleType.STUDENT).ToListAsync();

                foreach(var u in users)
                {
                    var episodes = await ctx.Episodes.Include(c => c.Course).Where(e => e.Course != null && e.Course.UserId != u.Id).ToListAsync();

                    foreach(var ep in episodes)
                    {
                        if (ep.IsPromo.HasValue && ep.IsPromo.Value)
                        {
                            //do not count promotional video
                        }
                        else
                        {
                            await CounTimeForEpisode(ep.Id, u.Id, (decimal)ep.MediaLenght);
                        }
                    }
                }
            }

        }

        public async Task CounTimeForEpisode(int episodeId, int userId, decimal epDuration)
        {
            var data = await _mongoCtx.UserEpisodes.FindByUserIdAndEpisodeId(episodeId, userId);

            if ( data== null || data.Count == 0) { return; }

            List<EpisodeTimestamp> timestamps = new List<EpisodeTimestamp>();

            data.ToList().ForEach(t =>
            {

                timestamps.AddRange(t.Timestamps);
            });


            timestamps = timestamps.Distinct().OrderBy(t => t.Value).ToList();

            var grouppedByDate = timestamps.GroupBy(t => t.UpdateTime.Date);

            using (var ctx = new DataContext())
            {
                foreach (var tim in grouppedByDate)
                {
                    var previousAndCurrentTimestamps = timestamps.Where(t => t.UpdateTime.Date <= tim.Key.Date).ToList();
                    var anyExists = await ctx.UserWatchedEpisodes.FirstOrDefaultAsync(t => t.UserId == userId && t.EpisodeId == episodeId && t.Day == tim.Key.Date);

                    if (anyExists != null && anyExists.IsWatched)
                    {

                    }
                    else
                    {

                        var dateTimestamps = previousAndCurrentTimestamps.Distinct().OrderBy(t => t.Value).ToList();

                        decimal watchedTime = 0;

                        for (int i = 1; i < dateTimestamps.Count; i++)
                        {

                            var previous = dateTimestamps[i - 1];

                            var current = dateTimestamps[i];

                            var val = current.Value - (previous.Value * previous.Rate);
                            if (val > 0 && val + (val * 0.1m) >= 5)
                            {
                                watchedTime += val;
                            }
     
                        }

                        if (watchedTime>0)
                        {
      
                                if (anyExists != null)
                                {
                                    anyExists.EpisodeWatchedTime = watchedTime;
                                    anyExists.EpisodeDuration = epDuration;
                                    if (watchedTime + (watchedTime * 0.1m) >= epDuration)
                                    {
                                        anyExists.IsWatched = true;
                                    }
                                    else
                                    {
                                        anyExists.IsWatched = false;
                                    }

                                    await ctx.SaveChangesAsync();
                                }
                                else
                                {

                                    UserWatchedEpisode ep = new UserWatchedEpisode();
                                    ep.Day = tim.Key.Date;
                                    ep.EpisodeDuration = epDuration;
                                    ep.EpisodeId = episodeId;
                                    ep.UserId = userId;
                                    ep.EpisodeWatchedTime = watchedTime;

                                    if (watchedTime + (watchedTime * 0.1m) >= epDuration)
                                    {
                                        ep.IsWatched = true;
                                    }
                                    else
                                    {
                                        ep.IsWatched = false;
                                    }

                                    ctx.UserWatchedEpisodes.Add(ep);
                                    await ctx.SaveChangesAsync();
                                }

                            
                        }
                    }
                }

            }

            //if (duration > 0)
            //{
            //    decimal watchedTime = 0;
            //    for (int i = 1; i < timestamps.Count; i++)
            //    {

            //        var previous = timestamps[i - 1];

            //        var current = timestamps[i];


            //        var val = current.Value - (previous.Value * previous.Rate);
            //        if (val > 0 && val + (val * 0.1m) >= 5)
            //        {
            //            watchedTime += val;
            //        }
            //    }

            //    if (watchedTime + (watchedTime * 0.05m) >= duration)
            //    {
            //        //episode watched!!!!
            //        //userid
            //        //episodeid
            //        //watchedminutes
            //    }
            //}




        }

        public async Task ReSuggestVideosForDay(DateTime day, int monthPeriod)
        {
            using(var ctx = new DataContext())
            {
                var toDelete = await ctx.SuggestedCourses.Where(t => t.CreationDay == day.Date).ToListAsync();

                ctx.SuggestedCourses.RemoveRange(toDelete);

                await ctx.SaveChangesAsync();

                await SuggestVideos(day.Date, monthPeriod);
            }
        }


        public async Task SuggestVideos(DateTime day, int monthPeriod)
        {
            var lastMth = day.AddMonths(-monthPeriod).Date;
            using (var ctx = new DataContext())
            {
                var suggestionExists = await ctx.SuggestedCourses.AnyAsync(t => t.CreationDay.Date == day.Date);
                if(suggestionExists)
                {
                    return;
                }
                var data = await ctx.UserWatchedEpisodes.Where(t => t.Day >= lastMth).ToListAsync();

                var grouppedByEpisode = data.GroupBy(t => t.EpisodeId);
                Dictionary<int, decimal> episodesByWatchTime = new Dictionary<int, decimal>();
                foreach (var g in grouppedByEpisode)
                {
                    var countTime = g.Sum(t => t.EpisodeWatchedTime);
                    episodesByWatchTime.Add(g.Key, countTime);
                }

                Dictionary<int, decimal> coursesByWatchTime = new Dictionary<int, decimal>();

                foreach (var ep in episodesByWatchTime)
                {
                    var episode = await ctx.Episodes.FirstOrDefaultAsync(t => t.Id == ep.Key);
                    if (episode != null)
                    {
                        if (coursesByWatchTime.ContainsKey(episode.CourseId))
                        {
                            coursesByWatchTime[episode.CourseId] += ep.Value;
                        }
                        else
                        {
                            coursesByWatchTime.Add(episode.CourseId, ep.Value);
                        }
                    }
                }

                var dataToSuggest = coursesByWatchTime.OrderByDescending(t => t.Value).Take(20);

                foreach (var d in dataToSuggest)
                {
                    var suggested = new SuggestedCourse();
                    suggested.CourseId = d.Key;
                    suggested.CreationDay = day.Date;
                    suggested.WatchedTime = d.Value;
                    ctx.SuggestedCourses.Add(suggested);
                    await ctx.SaveChangesAsync();
                }
            }
        }
    }
}
