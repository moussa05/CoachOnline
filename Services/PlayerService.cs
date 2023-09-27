using CoachOnline.ElasticSearch.Models;
using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Model.Student;
using CoachOnline.Mongo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Services
{
    public class PlayerService:IPlayerMedia
    {

        private MongoCtx _mongoCtx;

        public PlayerService(MongoCtx mongoCtx)
        {
            _mongoCtx = mongoCtx;
        }


        public async Task<List<CoachCategories>> GetCoachCategories(int coachId)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.Where(t => t.Id == coachId).Include(c => c.AccountCategories).FirstOrDefaultAsync();
                var categories = user.AccountCategories;
                List<CoachCategories> cats = new List<CoachCategories>();
                if (categories != null)
                {
                    categories.ForEach(el =>
                    {
                        cats.Add(new CoachCategories { Id = el.Id, Name = el.Name });
                    });
                }

                return cats;
            }
        }


        public async Task<CourseResponse> OpenCourse(int? userId, int courseId, bool activeSubscription)
        {
            using (var ctx = new DataContext())
            {
                var c = await ctx.courses.Where(t => t.Id == courseId).Include(e=>e.Evaluations).Include(c => c.Category).ThenInclude(p => p.Parent).Include(e => e.Episodes).ThenInclude(m => m.Attachments).FirstOrDefaultAsync();
                if (c == null)
                {
                    return null;
                }

                //bool hasPromo = c.HasPromo.HasValue && c.HasPromo.Value;

                //if (activeSubscription == false && hasPromo == false)
                //{
                //    throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                //}

                //if (!userId.HasValue || (userId.HasValue && !hasPromo && activeSubscription ==false))
                //{
                //    if (userId.HasValue)
                //    {
                //        var usr = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId.Value);
                //        if (usr != null)
                //        {
                //            if (!usr.AccountCreationDate.HasValue || (usr.AccountCreationDate.HasValue && usr.AccountCreationDate.Value.AddDays(3) >= DateTime.Now))
                //            {
                //                throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (!hasPromo)
                //        {
                //            throw new CoachOnlineException("Promo episode not exists", CoachOnlineExceptionState.NotExist);
                //        }
                //    }
                //}
                bool isLikedByMe = false;
                if (userId.HasValue)
                {
                    isLikedByMe = c.Evaluations == null ? false : c.Evaluations.Any(u => u.UserId == userId.Value && u.IsLiked);

                    var courseStats = await ctx.StudentOpenedCourses.FirstOrDefaultAsync(t => t.StudentId == userId.Value && t.CourseId == c.Id);
                    if (courseStats == null)
                    {
                        var opened = new StudentCourse();
                        opened.CourseId = c.Id;
                        opened.LastOpenedDate = DateTime.Now;
                        opened.StudentId = userId.Value;
                        opened.FirstOpenedDate = DateTime.Now;
                        ctx.StudentOpenedCourses.Add(opened);
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        courseStats.LastOpenedDate = DateTime.Now;
                        await ctx.SaveChangesAsync();
                    }
                }

                CourseResponse courseResponse = new CourseResponse();
                courseResponse.Episodes = new List<EpisodeResponse>();
                courseResponse.RejectionsHistory = new List<RejectionResponse>();
                courseResponse.Category = new CategoryAPI();
                courseResponse.Category.ParentsChildren = new List<CategoryAPI>();
                courseResponse.LikesCnt = c.Evaluations == null? 0: c.Evaluations.Count(x => x.IsLiked);
                courseResponse.Created = c.Created;
                courseResponse.IsLikedByMe = isLikedByMe;
                courseResponse.Description = c.Description;
                courseResponse.Id = c.Id;
                courseResponse.Name = c.Name ?? "";
                courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                courseResponse.State = c.State;
                courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                courseResponse.Coach = new CoachInfoResponse();
                courseResponse.Prerequisite = c.Prerequisite ?? "";
                courseResponse.Objectives = c.Objectives ?? "";
                courseResponse.PublicTargets = c.PublicTargets ?? "";
                courseResponse.CertificationQCM = c.CertificationQCM ?? "";

                var courseUser = await ctx.courses.Where(t => t.Id == c.Id).Include(u => u.User).ThenInclude(x => x.OwnedCourses).ThenInclude(cat=>cat.Category).FirstOrDefaultAsync();
                if(courseUser!= null)
                {
                    courseResponse.Coach.Bio = courseUser.User.Bio;
                    courseResponse.Coach.Country = courseUser.User.Country;
                    courseResponse.Coach.Email = courseUser.User.EmailAddress;
                    courseResponse.Coach.FirstName = courseUser.User.FirstName;
                    courseResponse.Coach.LastName = courseUser.User.Surname;
                    courseResponse.Coach.Id = courseUser.UserId;
                    courseResponse.Coach.Gender = courseUser.User.Gender;
                    courseResponse.Coach.YearOfBirth = courseUser.User.YearOfBirth;
                    courseResponse.Coach.UserCategories = await GetCoachCategories(courseUser.UserId);
                    courseResponse.Coach.PhotoUrl = courseUser.User.AvatarUrl;
                    courseResponse.Coach.Courses = new List<CourseResponse>();

                    foreach(var cr in courseUser.User.OwnedCourses)
                    {
                        // if (cr.Id != c.Id)
                        //{
                        if (cr.State == CourseState.APPROVED)
                        {
                            courseResponse.Coach.Courses.Add(new CourseResponse()
                            {
                                Description = cr.Description,
                                Category = new CategoryAPI() { Id = cr.CategoryId, Name = cr.Category.Name, AdultOnly = cr.Category.AdultOnly },
                                Name = cr.Name,
                                PhotoUrl = cr.PhotoUrl,
                                Id = cr.Id,
                                Created = cr.Created,
                                State = cr.State

                            });
                        }
                        //}
                    }
                }

                if (c.Episodes != null && c.Episodes.Count > 0)
                {
                    foreach (var e in c.Episodes)
                    {
                        EpisodeResponse episodeResponse = new EpisodeResponse();
                       
                        episodeResponse.Created = e.Created;
                        episodeResponse.Description = e.Description ?? "";
                        episodeResponse.Id = e.Id;
                        episodeResponse.MediaId = e.MediaId;
                        episodeResponse.OrdinalNumber = e.OrdinalNumber;
                        episodeResponse.Title = e.Title ?? "";
                        episodeResponse.CourseId = e.CourseId;
                        episodeResponse.Length = e.MediaLenght;
                        episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                        episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                        episodeResponse.EpisodeState = e.EpisodeState;
                        courseResponse.Episodes.Add(episodeResponse);



                    }

                    courseResponse.Episodes = courseResponse.Episodes.OrderBy(t => t.OrdinalNumber).ToList();
                }

                if (c.Category != null)
                {
                    courseResponse.Category.Name = c.Category.Name;
                    courseResponse.Category.Id = c.Category.Id;
                    courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                    if (c.Category.Parent != null)
                    {
                        courseResponse.Category.ParentId = c.Category.Parent.Id;
                        courseResponse.Category.ParentName = c.Category.Parent.Name;
                        if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                        {
                            foreach (var pc in c.Category.Parent.Children)
                            {
                                courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                            }
                        }
                    }
                }

                return courseResponse;

            }
        }

        private async Task<decimal> GetLastEpisodeTimestamp(int episodeId, int userId)
        {
            var episodeData = await _mongoCtx.UserEpisodes.FindByUserIdAndEpisodeId(episodeId, userId);
            decimal maxTimestamp = 0;
            episodeData.ForEach(x =>
            {
                maxTimestamp = x.Timestamps.Max(x => x.Value);
            });

            return maxTimestamp;
        }

        private async Task<decimal> GetEpisodeDuration(int episodeId, int userId)
        {
            //var episodeData = await _mongoCtx.UserEpisodes.FindByUserIdAndEpisodeId(episodeId, userId);
            //if (episodeData.Any())
            //{
            //    decimal duration = episodeData.Max(x => x.Duration);



            //    return duration;
            //}
            using(var ctx = new DataContext())
            {
                var episode = await ctx.Episodes.FirstOrDefaultAsync(t => t.Id == episodeId);
                if(episode != null)
                {
                    return (decimal)episode.MediaLenght;
                }
            }
            return 0;
        }

        private async Task<bool> IsEpisodeWatched(int episodeId, int userId)
        {

            var timestamp = await GetLastEpisodeTimestamp(episodeId, userId);
            var duration = await GetEpisodeDuration(episodeId, userId);
            if (duration > 0 && timestamp + (timestamp * 0.05m) >= duration)
            {
                return true;
            }


            return false;

        }

        private async Task<Tuple<decimal, decimal>> GetCourseWatchedPercentage(int courseId, int userId)
        {
            using(var ctx = new DataContext())
            {
                var allEpisodes = await ctx.Episodes.Where(t => t.CourseId == courseId).ToListAsync();
                int watched = 0;
                int episodesCnt = allEpisodes.Count;
                foreach(var ep in allEpisodes)
                {

                    if(await IsEpisodeWatched(ep.Id, userId))
                    {
                        watched++;
                    }
                }

                return new Tuple<decimal, decimal>(watched, episodesCnt);
            }
        }

        public async Task<EpisodeResponse> OpenEpisode(int? userId, int episodeId, bool activeSubscription)
        {
            using (var ctx = new DataContext())
            {
                var e = await ctx.Episodes.Where(t => t.Id == episodeId).Include(x => x.Attachments).FirstOrDefaultAsync();
                if (e == null)
                {
                    return null;
                }

                bool isPromo = e.IsPromo.HasValue && e.IsPromo.Value;

                if(userId.HasValue && activeSubscription == false && isPromo == false)
                {
                    throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                }

                //if(!userId.HasValue)
                //{
                //    throw new CoachOnlineException("User is not authorized", CoachOnlineExceptionState.NotAuthorized);
                //}

                if(!isPromo && !activeSubscription)
                {
                    throw new CoachOnlineException("Episode is not a promo episode", CoachOnlineExceptionState.PermissionDenied);
                }

                if (!userId.HasValue || (userId.HasValue && isPromo && activeSubscription == false))
                {
                    if (userId.HasValue)
                    {
                        var usr = await ctx.users.FirstOrDefaultAsync(x => x.Id == userId.Value);
                        if (!usr.AccountCreationDate.HasValue || (usr.AccountCreationDate.HasValue && usr.AccountCreationDate.Value.AddDays(7) < DateTime.Now))
                        {
                            throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                        }
                    }
                }

                decimal stoppedAtTimestamp = 0;
                if (userId.HasValue)
                {

                    var openedEp = await ctx.StudentOpenedEpisodes.Where(t => t.StudentId == userId.Value && t.EpisodeId == episodeId).FirstOrDefaultAsync();
                    if (openedEp != null)
                    {

                        if (openedEp.WatchedStatus != WatchStatus.WATCHED)
                        {
                            if (await IsEpisodeWatched(episodeId, userId.Value))
                            {
                                openedEp.WatchedStatus = WatchStatus.WATCHED;
                            }
                            else
                            {
                                openedEp.WatchedStatus = WatchStatus.IN_PROGRESS;
                            }
                        }
                        openedEp.LastWatchDate = DateTime.Now;
                        openedEp.StoppedAtTimestamp = await GetLastEpisodeTimestamp(e.Id, userId.Value);
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        openedEp = new StudentEpisode();
                        openedEp.WatchedStatus = WatchStatus.OPENED;
                        openedEp.StudentId = userId.Value;
                        openedEp.CourseId = e.CourseId;
                        openedEp.EpisodeId = e.Id;
                        openedEp.LastWatchDate = DateTime.Now;
                        openedEp.StoppedAtTimestamp = await GetLastEpisodeTimestamp(e.Id, userId.Value);
                        openedEp.FirstOpenDate = DateTime.Now;
                        openedEp.Duration = (decimal)e.MediaLenght;

                        ctx.StudentOpenedEpisodes.Add(openedEp);
                        await ctx.SaveChangesAsync();
                    }

                    stoppedAtTimestamp = openedEp.StoppedAtTimestamp;
                }
                //TODO

                var epResp = new EpisodeResponse();
                epResp.Attachments = await GetEpisodeAttachments(e.Id);
                epResp.CourseId = e.CourseId;
                epResp.Created = e.Created;
                epResp.Description = e.Description;
                epResp.Id = e.Id;
                epResp.MediaId = e.MediaId;
                epResp.OrdinalNumber = e.OrdinalNumber;
                epResp.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                epResp.Title = e.Title;
                if (userId.HasValue)
                {
                    epResp.Query = await GetAttachmentPermission(userId.Value, episodeId);
                }
                else
                {
                    epResp.Query = await GetAttachmentPermissionForPromoAndUnauthorizedUser(episodeId);
                }
                epResp.LastOpenedSecond = stoppedAtTimestamp > (decimal)e.MediaLenght? (decimal)e.MediaLenght: stoppedAtTimestamp;
                epResp.Length = e.MediaLenght;
                epResp.EpisodeState = e.EpisodeState;
                return epResp;

            }
        
        }

        public async Task<int> EvalCourse(int courseId, int userId, bool isLiked)
        {
            using(var ctx = new DataContext())
            {
                var course = await ctx.courses.Where(x => x.Id == courseId).Include(l => l.Evaluations).FirstOrDefaultAsync();
                course.CheckExist("Course");

                if(course.Evaluations == null)
                {
                    course.Evaluations = new List<CourseEval>();
                }

                var exists = course.Evaluations.Where(x => x.CourseId == courseId && x.UserId == userId).FirstOrDefault();

                if(exists!= null)
                {
                    exists.IsLiked = isLiked;
                }
                else
                {
                    course.Evaluations.Add(new CourseEval { UserId = userId, CourseId = courseId, IsLiked = isLiked });

                   
                }

                await ctx.SaveChangesAsync();
                return course.Evaluations.Count(x => x.IsLiked);
            }
        }

        public async Task<bool> IsEpisodeAPromo(int episodeId)
        {
            using (var ctx = new DataContext())
            {
                var ep = await ctx.Episodes.FirstOrDefaultAsync(x => x.Id == episodeId);
                if(ep!=null)
                {

                    var result = ep.IsPromo.HasValue && ep.IsPromo.Value;

                    return result;
                }

                return false;
            }
        }

        public async Task<List<CourseResponse>> GetSuggestedCourses()
        {
            using(var ctx = new DataContext())
            {
                var suggested = await ctx.SuggestedCourses.Where(t => t.CreationDay.Date == DateTime.Today.Date).ToListAsync();
                if(!suggested.Any())
                {
                    var dayBefore = DateTime.Today.AddDays(-1).Date;
                    suggested = await ctx.SuggestedCourses.Where(t => t.CreationDay.Date == dayBefore.Date).ToListAsync();
                }
                var dataList = new List<CourseResponse>();
                if (suggested.Any())
                {
                    foreach(var s in suggested)
                    {
                        var c = await ctx.courses.Where(t => t.Id == s.CourseId && t.State == CourseState.APPROVED).Include(e=>e.Evaluations).Include(c => c.Category).ThenInclude(p => p.Parent).Include(e => e.Episodes).FirstOrDefaultAsync();
                        if (c != null)
                        {
                            CourseResponse courseResponse = new CourseResponse();
                            courseResponse.Episodes = new List<EpisodeResponse>();
                            courseResponse.RejectionsHistory = new List<RejectionResponse>();
                            courseResponse.Category = new CategoryAPI();
                            courseResponse.Category.ParentsChildren = new List<CategoryAPI>();
                            courseResponse.Created = c.Created;
                            courseResponse.Description = c.Description;
                            courseResponse.Id = c.Id;
                            courseResponse.Name = c.Name ?? "";
                            courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                            courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                            courseResponse.State = c.State;
                            courseResponse.Coach = new CoachInfoResponse();
                            courseResponse.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;
                            var courseUser = await ctx.courses.Where(t => t.Id == c.Id).Include(u => u.User).ThenInclude(x => x.OwnedCourses).ThenInclude(cat => cat.Category).FirstOrDefaultAsync();
                            if (courseUser != null)
                            {
                                courseResponse.Coach.Bio = courseUser.User.Bio;
                                courseResponse.Coach.Country = courseUser.User.Country;
                                courseResponse.Coach.Email = courseUser.User.EmailAddress;
                                courseResponse.Coach.FirstName = courseUser.User.FirstName;
                                courseResponse.Coach.LastName = courseUser.User.Surname;
                                courseResponse.Coach.Gender = courseUser.User.Gender;
                                courseResponse.Coach.Id = courseUser.UserId;
                                courseResponse.Coach.YearOfBirth = courseUser.User.YearOfBirth;
                                courseResponse.Coach.UserCategories = await GetCoachCategories(courseUser.UserId);
                                courseResponse.Coach.PhotoUrl = courseUser.User.AvatarUrl;
                                courseResponse.Coach.Courses = new List<CourseResponse>();

                                foreach (var cr in courseUser.User.OwnedCourses)
                                {
                                    // if (cr.Id != c.Id)
                                    //{
                                    if (cr.State == CourseState.APPROVED)
                                    {
                                        courseResponse.Coach.Courses.Add(new CourseResponse()
                                        {
                                            Description = cr.Description,
                                            Category = new CategoryAPI() { Id = cr.CategoryId, Name = cr.Category.Name, AdultOnly = cr.Category.AdultOnly },
                                            Name = cr.Name,
                                            PhotoUrl = cr.PhotoUrl,
                                            Id = cr.Id,
                                            Created = cr.Created,
                                            State = cr.State

                                        });
                                    }
                                    //}
                                }
                            }

                            if (c.Episodes != null && c.Episodes.Count > 0)
                            {
                                foreach (var e in c.Episodes)
                                {
                                    EpisodeResponse episodeResponse = new EpisodeResponse();

                                    episodeResponse.Created = e.Created;
                                    episodeResponse.Description = e.Description ?? "";
                                    episodeResponse.Id = e.Id;
                                    episodeResponse.MediaId = e.MediaId;
                                    episodeResponse.OrdinalNumber = e.OrdinalNumber;
                                    episodeResponse.Title = e.Title ?? "";
                                    episodeResponse.CourseId = e.CourseId;
                                    episodeResponse.Length = e.MediaLenght;
                                    episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                                    episodeResponse.EpisodeState = e.EpisodeState;
                                    courseResponse.Episodes.Add(episodeResponse);



                                }
                            }

                            if (c.Category != null)
                            {
                                courseResponse.Category.Name = c.Category.Name;
                                courseResponse.Category.Id = c.Category.Id;
                                courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                                if (c.Category.Parent != null)
                                {
                                    courseResponse.Category.ParentId = c.Category.Parent.Id;
                                    courseResponse.Category.ParentName = c.Category.Parent.Name;
                                    if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                                    {
                                        foreach (var pc in c.Category.Parent.Children)
                                        {
                                            courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                                        }
                                    }
                                }
                            }
                            dataList.Add(courseResponse);
                        }
                    }
                }

                return dataList;
            }
        }


        public async Task<List<CourseResponse>> GetMostTrendingCourses()
        {
            using (var ctx = new DataContext())
            {
                var mostTrending = await ctx.courses.Where(s => s.State == CourseState.APPROVED).Include(l => l.Evaluations).Where(x=> x.Evaluations != null && x.Evaluations.Any()).ToListAsync();

                var temp = mostTrending.OrderByDescending(x => x.Evaluations.Count(t => t.IsLiked)).ThenByDescending(x=>x.Created).Take(20).ToList();

                var dataList = new List<CourseResponse>();
                if (temp.Any())
                {
                    foreach (var s in temp)
                    {
                        var c = await ctx.courses.Where(t => t.Id == s.Id && t.State == CourseState.APPROVED).Include(c => c.Category).ThenInclude(p => p.Parent).Include(e => e.Episodes).FirstOrDefaultAsync();
                        if (c != null)
                        {
                            CourseResponse courseResponse = new CourseResponse();
                            courseResponse.Episodes = new List<EpisodeResponse>();
                            courseResponse.RejectionsHistory = new List<RejectionResponse>();
                            courseResponse.Category = new CategoryAPI();
                            courseResponse.Category.ParentsChildren = new List<CategoryAPI>();
                            courseResponse.Created = c.Created;
                            courseResponse.Description = c.Description;
                            courseResponse.Id = c.Id;
                            courseResponse.Name = c.Name ?? "";
                            courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                            courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                            courseResponse.State = c.State;
                            courseResponse.Coach = new CoachInfoResponse();
                            courseResponse.LikesCnt = s.Evaluations == null ? 0 : s.Evaluations.Count(x => x.IsLiked);

                            var courseUser = await ctx.courses.Where(t => t.Id == c.Id).Include(u => u.User).ThenInclude(x => x.OwnedCourses).ThenInclude(cat => cat.Category).FirstOrDefaultAsync();
                            if (courseUser != null)
                            {
                                courseResponse.Coach.Bio = courseUser.User.Bio;
                                courseResponse.Coach.Country = courseUser.User.Country;
                                courseResponse.Coach.Email = courseUser.User.EmailAddress;
                                courseResponse.Coach.FirstName = courseUser.User.FirstName;
                                courseResponse.Coach.LastName = courseUser.User.Surname;
                                courseResponse.Coach.Gender = courseUser.User.Gender;
                                courseResponse.Coach.Id = courseUser.UserId;
                                courseResponse.Coach.YearOfBirth = courseUser.User.YearOfBirth;
                                courseResponse.Coach.UserCategories = await GetCoachCategories(courseUser.UserId);
                                courseResponse.Coach.PhotoUrl = courseUser.User.AvatarUrl;
                                courseResponse.Coach.Courses = new List<CourseResponse>();

                                foreach (var cr in courseUser.User.OwnedCourses)
                                {
                                    // if (cr.Id != c.Id)
                                    //{
                                    if (cr.State == CourseState.APPROVED)
                                    {
                                        courseResponse.Coach.Courses.Add(new CourseResponse()
                                        {
                                            Description = cr.Description,
                                            Category = new CategoryAPI() { Id = cr.CategoryId, Name = cr.Category.Name, AdultOnly = cr.Category.AdultOnly },
                                            Name = cr.Name,
                                            PhotoUrl = cr.PhotoUrl,
                                            Id = cr.Id,
                                            Created = cr.Created,
                                            State = cr.State

                                        });
                                    }
                                    //}
                                }
                            }

                            if (c.Episodes != null && c.Episodes.Count > 0)
                            {
                                foreach (var e in c.Episodes)
                                {
                                    EpisodeResponse episodeResponse = new EpisodeResponse();

                                    episodeResponse.Created = e.Created;
                                    episodeResponse.Description = e.Description ?? "";
                                    episodeResponse.Id = e.Id;
                                    episodeResponse.MediaId = e.MediaId;
                                    episodeResponse.OrdinalNumber = e.OrdinalNumber;
                                    episodeResponse.Title = e.Title ?? "";
                                    episodeResponse.CourseId = e.CourseId;
                                    episodeResponse.Length = e.MediaLenght;
                                    episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                                    episodeResponse.EpisodeState = e.EpisodeState;
                                    courseResponse.Episodes.Add(episodeResponse);



                                }
                            }

                            if (c.Category != null)
                            {
                                courseResponse.Category.Name = c.Category.Name;
                                courseResponse.Category.Id = c.Category.Id;
                                courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                                if (c.Category.Parent != null)
                                {
                                    courseResponse.Category.ParentId = c.Category.Parent.Id;
                                    courseResponse.Category.ParentName = c.Category.Parent.Name;
                                    if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                                    {
                                        foreach (var pc in c.Category.Parent.Children)
                                        {
                                            courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                                        }
                                    }
                                }
                            }
                            dataList.Add(courseResponse);
                        }
                    }
                }

                return dataList;
            }
        }


        public async Task<List<CourseResponse>> GetFlaggedCourses()
        {
            using(var ctx = new DataContext())
            {
                var courses = await ctx.FlaggedCourses.OrderBy(t=>t.OrderNo).ToListAsync();
                var dataList = new List<CourseResponse>();

                foreach (var crs in courses)
                {
                    var c = await ctx.courses.Where(t => t.Id == crs.CourseId && t.State == CourseState.APPROVED).Include(e=>e.Evaluations).Include(c=>c.Category).ThenInclude(p=>p.Parent).Include(e=>e.Episodes).FirstOrDefaultAsync();
                    if (c != null)
                    {
                        CourseResponse courseResponse = new CourseResponse();
                        courseResponse.Episodes = new List<EpisodeResponse>();
                        courseResponse.RejectionsHistory = new List<RejectionResponse>();
                        courseResponse.Category = new CategoryAPI();
                        courseResponse.Category.ParentsChildren = new List<CategoryAPI>();
                        courseResponse.IsFlagged = true;
                        courseResponse.OrderNo = crs.OrderNo;
                        courseResponse.Created = c.Created;
                        courseResponse.Description = c.Description;
                        courseResponse.Id = c.Id;
                        courseResponse.Name = c.Name ?? "";
                        courseResponse.PhotoUrl = c.PhotoUrl ?? "";
                        courseResponse.State = c.State;
                        courseResponse.BannerPhotoUrl = c.BannerPhotoUrl ?? "";
                        courseResponse.Coach = new CoachInfoResponse();
                        courseResponse.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;

                        var courseUser = await ctx.courses.Where(t => t.Id == c.Id).Include(u => u.User).ThenInclude(x => x.OwnedCourses).ThenInclude(cat => cat.Category).FirstOrDefaultAsync();
                        if (courseUser != null)
                        {
                            courseResponse.Coach.Bio = courseUser.User.Bio;
                            courseResponse.Coach.Country = courseUser.User.Country;
                            courseResponse.Coach.Email = courseUser.User.EmailAddress;
                            courseResponse.Coach.FirstName = courseUser.User.FirstName;
                            courseResponse.Coach.LastName = courseUser.User.Surname;
                            courseResponse.Coach.Gender = courseUser.User.Gender;
                            courseResponse.Coach.Id = courseUser.UserId;
                            courseResponse.Coach.YearOfBirth = courseUser.User.YearOfBirth;
                            courseResponse.Coach.UserCategories = await GetCoachCategories(courseUser.UserId);
                            courseResponse.Coach.PhotoUrl = string.IsNullOrEmpty(courseUser.User.AvatarUrl) ? "" : $"images/{courseUser.User.AvatarUrl}";
                            courseResponse.Coach.Courses = new List<CourseResponse>();

                            foreach (var cr in courseUser.User.OwnedCourses)
                            {
                                // if (cr.Id != c.Id)
                                //{
                                if (cr.State == CourseState.APPROVED)
                                {
                                    courseResponse.Coach.Courses.Add(new CourseResponse()
                                    {
                                        Description = cr.Description,
                                        Category = new CategoryAPI() { Id = cr.CategoryId, Name = cr.Category.Name, AdultOnly = cr.Category.AdultOnly },
                                        Name = cr.Name,
                                        PhotoUrl = cr.PhotoUrl,
                                        Id = cr.Id,
                                        Created = cr.Created,
                                        State = cr.State

                                    });
                                }
                                //}
                            }
                        }

                        if (c.Episodes != null && c.Episodes.Count > 0)
                        {
                            foreach (var e in c.Episodes)
                            {
                                EpisodeResponse episodeResponse = new EpisodeResponse();

                                episodeResponse.Created = e.Created;
                                episodeResponse.Description = e.Description ?? "";
                                episodeResponse.Id = e.Id;
                                episodeResponse.MediaId = e.MediaId;
                                episodeResponse.OrdinalNumber = e.OrdinalNumber;
                                episodeResponse.Title = e.Title ?? "";
                                episodeResponse.CourseId = e.CourseId;
                                episodeResponse.Length = e.MediaLenght;
                                episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                                episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                                episodeResponse.EpisodeState = e.EpisodeState;
                                courseResponse.Episodes.Add(episodeResponse);



                            }
                        }

                        if (c.Category != null)
                        {
                            courseResponse.Category.Name = c.Category.Name;
                            courseResponse.Category.Id = c.Category.Id;
                            courseResponse.Category.AdultOnly = c.Category.AdultOnly;

                            if (c.Category.Parent != null)
                            {
                                courseResponse.Category.ParentId = c.Category.Parent.Id;
                                courseResponse.Category.ParentName = c.Category.Parent.Name;
                                if (c.Category.Parent.Children != null && c.Category.Parent.Children.Count > 0)
                                {
                                    foreach (var pc in c.Category.Parent.Children)
                                    {
                                        courseResponse.Category.ParentsChildren.Add(new CategoryAPI { Id = pc.Id, Name = pc.Name });
                                    }
                                }
                            }
                        }
                        dataList.Add(courseResponse);
                    }
                }

                return dataList;
            }
        }

        public async Task<List<EpisodeAttachment>> GetEpisodeAttachments(int episodeId)
        {
            var attachments = new List<EpisodeAttachment>();
            using (var ctx = new DataContext())
            {
                var att = await ctx.Episodes.Where(t => t.Id == episodeId).Include(a => a.Attachments).FirstOrDefaultAsync();
                if (att != null && att.Attachments != null && att.Attachments.Count > 0)
                {
                    foreach (var a in att.Attachments)
                    {
                        var x = new EpisodeAttachment();
                        x.Extension = a.Extension;
                        x.Hash = a.Hash;
                        x.Id = a.Id;
                        x.Name = a.Name;
                        x.Added = a.Added;
                        x.QueryString = $"attachments/{a.Hash}.{a.Extension}";

                        attachments.Add(x);
                    }
                }
            }
            return attachments;
        }

        private async Task<CoachInfoResponse> GetCoachResp(int userId)
        {
            CoachInfoResponse resp = new CoachInfoResponse();

            using(var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(c => c.Id == userId);
                if(user!= null)
                {
                    resp.Bio = user.Bio;
                    resp.Country = user.Country;
                    resp.Email = user.EmailAddress;
                    resp.FirstName = user.FirstName;
                    resp.LastName = user.Surname;
                    resp.PhotoUrl = user.AvatarUrl;
                    resp.Gender = user.Gender;
                    resp.Id = user.Id;
                    resp.YearOfBirth = user.YearOfBirth;
                }
            }

            return resp;
        }

        private async Task<bool> IsCourseFlagged(int courseId)
        {
            using(var ctx = new DataContext())
            {
                var isFlagged = await ctx.FlaggedCourses.AnyAsync(t => t.CourseId == courseId);

                return isFlagged;
            }
        }

        public async Task<ICollection<CourseResponse>> LastAddedCourses(int? userId)
        {
            using (var ctx = new DataContext())
            {
                var courses = await ctx.courses.Where(t=>t.State == CourseState.APPROVED).OrderByDescending(t => t.Created).Take(20).Include(e=>e.Evaluations).Include(c=>c.Category).Include(e=>e.Episodes).ToListAsync();
                var result = new List<CourseResponse>();

                foreach (var c in courses)
                {
                    //var openedEpisodes = new List<EpisodeResponse>();
                    //var percentage = new Tuple<decimal, decimal>(0, 0);
                    //if (userId.HasValue)
                    //{
                    //    openedEpisodes = (await LastOpenedEpisodesInCourse(userId.Value, c.Id)).ToList();
                    //    percentage = await GetCourseWatchedPercentage(c.Id, userId.Value);
                    //}
                    CourseResponse resp = new CourseResponse();
                    resp.Created = c.Created;
                    resp.Description = c.Description;
                    resp.Id = c.Id;
                    resp.Name = c.Name;
                    resp.PhotoUrl = c.PhotoUrl;
                    resp.BannerPhotoUrl = c.BannerPhotoUrl;
                    resp.Episodes = new List<EpisodeResponse>();
                    //resp.AllEpisodesCnt = c.Episodes.Count;
                    resp.IsFlagged = await IsCourseFlagged(c.Id);
                    resp.LikesCnt = c.Evaluations != null ? c.Evaluations.Count(x => x.IsLiked) : 0;

                    //if (percentage.Item2 > 0)
                    //{
                    //    resp.WatchedPercentage = Math.Round((percentage.Item1 / percentage.Item2) * 100);
                    //}
                    //else
                    //{
                    //    resp.WatchedPercentage = 0;
                    //}

                    //resp.WatchedEpisodesCnt = percentage.Item1;
                    resp.Category = new CategoryAPI()
                    {
                        Name = c.Category.Name,
                        AdultOnly = c.Category.AdultOnly,
                        Id = c.Category.Id,
                        ParentId = c.Category.ParentId.HasValue ? c.Category.ParentId.Value : 0,
                        ParentName = c.Category.Parent?.Name
                    };

                    resp.Coach = await GetCoachResp(c.UserId);

                    resp.Episodes = new List<EpisodeResponse>();
                    if (c.Episodes != null && c.Episodes.Count > 0)
                    {
                        foreach (var e in c.Episodes)
                        {
                            EpisodeResponse episodeResponse = new EpisodeResponse();

                            episodeResponse.Created = e.Created;
                            episodeResponse.Description = e.Description ?? "";
                            episodeResponse.Id = e.Id;
                            episodeResponse.MediaId = e.MediaId;
                            episodeResponse.OrdinalNumber = e.OrdinalNumber;
                            episodeResponse.Title = e.Title ?? "";
                            episodeResponse.CourseId = e.CourseId;
                            episodeResponse.Length = e.MediaLenght;
                            episodeResponse.Attachments = await GetEpisodeAttachments(e.Id);
                            episodeResponse.IsPromo = e.IsPromo.HasValue && e.IsPromo.Value;
                            episodeResponse.EpisodeState = e.EpisodeState;
                            resp.Episodes.Add(episodeResponse);



                        }

                        resp.Episodes = resp.Episodes.OrderBy(t => t.OrdinalNumber).ToList();
                    }

                    result.Add(resp);


                }

                return result;
            }
        }

        public async Task<ICollection<CourseResponseWithWatchedStatus>> LastOpenedCourses(int userId)
        {
            using(var ctx = new DataContext())
            {

                var data = await ctx.StudentOpenedCourses.Where(t => t.StudentId == userId).Include(c => c.Course).ThenInclude(c=>c.Category).ThenInclude(p=>p.Parent).OrderByDescending(t => t.LastOpenedDate).ToListAsync();

                var result = new List<CourseResponseWithWatchedStatus>();
             
                foreach (var d in data)
                {
                    if (d.Course != null)
                    {
                        if (d.Course.State == CourseState.APPROVED)
                        {
                            var openedEpisodes = await LastOpenedEpisodesInCourse(userId, d.CourseId);
                            var percentage = await GetCourseWatchedPercentage(d.CourseId, userId);
                            CourseResponseWithWatchedStatus resp = new CourseResponseWithWatchedStatus();
                            resp.Created = d.Course.Created;
                            resp.Description = d.Course.Description;
                            resp.Id = d.Course.Id;
                            resp.Name = d.Course.Name;
                            resp.PhotoUrl = d.Course.PhotoUrl;
                            resp.State = d.Course.State;
                            var evals = await ctx.CourseEvals.Where(c => c.CourseId == d.CourseId).ToListAsync();
                            resp.LikesCnt = evals != null ? evals.Count(x => x.IsLiked) : 0;
                            resp.Episodes = openedEpisodes.ToList();
                            if (percentage.Item2 > 0)
                            {
                                resp.WatchedPercentage = Math.Round((percentage.Item1 / percentage.Item2) * 100);
                            }
                            else
                            {
                                resp.WatchedPercentage = 0;
                            }

                            resp.WatchedEpisodesCnt = percentage.Item1;
                            resp.AllEpisodesCnt = percentage.Item2;
                            resp.Category = new CategoryAPI()
                            {
                                Name = d.Course.Category.Name,
                                AdultOnly = d.Course.Category.AdultOnly,
                                Id = d.Course.Category.Id,
                                ParentId = d.Course.Category.ParentId.HasValue ? d.Course.Category.ParentId.Value : 0,
                                ParentName = d.Course.Category.Parent?.Name
                            };

                            resp.Coach = await GetCoachResp(d.Course.UserId);

                            result.Add(resp);
                        }
                    }
                }

                return result;
            }
        }

        public async Task<ICollection<EpisodeResponse>> LastOpenedEpisodesInCourse(int userId, int courseId)
        {
            var result = new List<EpisodeResponse>();
            using (var ctx = new DataContext())
            {
                var data = await ctx.StudentOpenedEpisodes.Where(t => t.StudentId == userId && t.CourseId == courseId).Include(e=>e.Episode).Include(c => c.Course).OrderByDescending(t => t.LastWatchDate).ToListAsync();
            
                foreach(var d in data)
                {
                    var ep = new EpisodeResponse();
                    ep.CourseId = d.CourseId;
                    ep.Created = d.Episode.Created;
                    ep.Description = d.Episode.Description;
                    ep.Id = d.EpisodeId;
                    ep.MediaId = d.Episode.MediaId;
                    ep.OrdinalNumber = d.Episode.OrdinalNumber;
                    ep.Title = d.Episode.Title;
                    ep.Attachments = await GetEpisodeAttachments(d.EpisodeId);
                    ep.LastOpenedSecond = await GetLastEpisodeTimestamp(d.EpisodeId, userId);
                    ep.Length = d.Episode.MediaLenght;
                    ep.EpisodeState = d.Episode.EpisodeState;
                    result.Add(ep);
                }
            }

            return result;
        }


        public async Task<string> GetAttachmentPermission(int userId, int episodeId)
        {
            using (var ctx = new DataContext())
            {
                var episode = await ctx.Episodes.Where(t => t.Id == episodeId).FirstOrDefaultAsync();
                episode.CheckExist("Episode");

                var userTokens = await ctx.users.Where(u => u.Id == userId).Include(tok => tok.UserLogins).FirstOrDefaultAsync();

                var lastToken = userTokens.UserLogins.OrderByDescending(t => t.Created).FirstOrDefault();

                var UserTokenPermission = await GenerateUserTokenForEpisodeMedia(userId, episodeId);
                var QueryString = $"uploads/{episode.MediaId}?Token={UserTokenPermission.CurrentToken}&Id={episodeId}&AuthToken={lastToken.AuthToken}";

                return QueryString;
            }
        }

        public async Task<string> GetAttachmentPermissionForPromoAndUnauthorizedUser(int episodeId)
        {
            using (var ctx = new DataContext())
            {
                var episode = await ctx.Episodes.Where(t => t.Id == episodeId).FirstOrDefaultAsync();
                episode.CheckExist("Episode");

                if(!episode.IsPromo.HasValue || !episode.IsPromo.Value)
                {
                    return "";
                }

                var QueryString = $"uploads/{episode.MediaId}?Id={episodeId}";

                return QueryString;
            }
        }

        private async Task<UserEpisodeAttachemntPermission> GenerateUserTokenForEpisodeMedia(int userId, int episodeId)
        {
            var token = Helpers.UrlToken.GenerateToken();
            using (var ctx = new DataContext())
            {
                var permission = await ctx.UserEpisodeAttachemntPermissions.FirstOrDefaultAsync(t => t.UserId == userId && t.MediaId == episodeId);

                if(permission != null)
                {
                    permission.CurrentToken = token;
                }
                else
                {
                    
                    permission = new UserEpisodeAttachemntPermission()
                    { CurrentToken = token, MediaId = episodeId, UserId = userId, CreationDate = DateTime.Now };
                    ctx.UserEpisodeAttachemntPermissions.Add(permission);
                }

                await ctx.SaveChangesAsync();

                return permission;
            }
           
        }


        public async Task<string> GetUserTokenForEpisodeMedia(int userId, int attachmentId, string token)
        {
            using (var ctx = new DataContext())
            {
                var permission = await ctx.UserEpisodeAttachemntPermissions.FirstOrDefaultAsync(t => t.UserId == userId && t.MediaId == attachmentId && t.CurrentToken == token);
                if (permission != null)
                {
                    //permission.CheckExist("Permission");
                    if (string.IsNullOrEmpty(permission.CurrentToken))
                    {
                        return null;
                    }
                    return permission.CurrentToken;
                }
                return null;
            }
        }

        public async Task DisposeUserTokenForEpisodeMedia(int userId, int episodeId)
        {
            using (var ctx = new DataContext())
            {
                var permission = await ctx.UserEpisodeAttachemntPermissions.Where(t => t.UserId == userId && t.MediaId == episodeId).ToListAsync();
                if(permission != null && permission.Count >0)
                {

                    foreach(var perm in permission)
                    {
                        if(!perm.CreationDate.HasValue || perm.CreationDate.Value.AddHours(2) < DateTime.Now)
                        {
                            ctx.UserEpisodeAttachemntPermissions.Remove(perm);
                        }
                    }              
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DisposeAllExpiredTokens()
        {
            using (var ctx = new DataContext())
            {
                var permission = await ctx.UserEpisodeAttachemntPermissions.ToListAsync();
                if (permission != null && permission.Count > 0)
                {

                    foreach (var perm in permission)
                    {
                        if (!perm.CreationDate.HasValue || perm.CreationDate.Value.AddHours(2) < DateTime.Now)
                        {
                            ctx.UserEpisodeAttachemntPermissions.Remove(perm);
                        }
                    }
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task<bool> UserIsAttachmentOwner(string attachment_hash, int user_id)
        {
            using (var ctx = new DataContext())
            {
                var result = await ctx.courses.Where(t => t.UserId == user_id).Include(e => e.Episodes).FirstOrDefaultAsync();
                bool isOwner = false;
                if (result.Episodes != null)
                {
                    foreach (var ep in result.Episodes)
                    {
                     
                        if(ep.MediaId == attachment_hash)
                        {
                            isOwner = true;
                            break;
                        }
                    }
                }

                return isOwner;
            }
        }
    }
}
