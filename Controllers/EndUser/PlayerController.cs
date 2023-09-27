using CoachOnline.Helpers;
using CoachOnline.Hubs;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.EndUser
{

    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        ILogger<PlayerController> _logger;
        IHttpContextAccessor _httpContextAccessor;
        private readonly IUser _userSvc;
        private readonly IPlayerMedia _playerMedia;
        private readonly ISubscription _subscriptionSvc;
        private readonly IComment _commentSvc;
        public PlayerController(ILogger<PlayerController> logger, IUser userSvc, IHttpContextAccessor httpContextAccessor, IPlayerMedia playerMedia, ISubscription subscriptionSvc, IComment commentSvc)
        {
            _logger = logger;
            _userSvc = userSvc;
            _httpContextAccessor = httpContextAccessor;
            _playerMedia = playerMedia;
            _subscriptionSvc = subscriptionSvc;
            _commentSvc = commentSvc;
        }

        [SwaggerResponse(200, Type = typeof(List<EpisodeAttachment>))]
        [HttpGet]
        public async Task<IActionResult> GetEpisodeAttachments(int courseId, int episodeId)
        {
            try
            {
                var userId = User.GetUserId();
                if(!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                using(var ctx = new DataContext())
                {
                    var course = await ctx.courses.Where(c => c.Id == courseId).Include(e => e.Episodes.Where(t=>t.Id == episodeId)).ThenInclude(a=>a.Attachments).FirstOrDefaultAsync();
                    course.CheckExist("Course");

                    var episode = course.Episodes.FirstOrDefault();
                    episode.CheckExist("Episode");
                    var attachments = new List<EpisodeAttachment>();
                    //episode.Attachments.ForEach(async a =>
                    //{
                    //    var gen = await _playerMedia.GetAttachmentPermission(userId.Value, episodeId);
                    //    attachments.Add(gen);
                    //});
                   
                    return new OkObjectResult(attachments);
                }
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(List<CourseResponse>))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetFlaggedCourses()
        {
            try
            {
                var flagged = await _playerMedia.GetFlaggedCourses();

                return new OkObjectResult(flagged);
                
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(List<CourseResponse>))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetSuggestedCourses()
        {
            try
            {
                var data = await _playerMedia.GetSuggestedCourses();

                return new OkObjectResult(data);

            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(List<CourseResponse>))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetMostTrendingCourses()
        {
            try
            {
                var data = await _playerMedia.GetMostTrendingCourses();

                return new OkObjectResult(data);

            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(string))]
        [HttpGet]
        public async Task<IActionResult> GetEpisodeMedia(int courseId, int episodeId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                using (var ctx = new DataContext())
                {
                    var course = await ctx.courses.Where(c => c.Id == courseId).Include(e => e.Episodes.Where(t => t.Id == episodeId && t.MediaId != null)).ThenInclude(a=>a.Attachments).FirstOrDefaultAsync();
                    course.CheckExist("Course");
                    
                    var episode = course.Episodes.FirstOrDefault();
                    episode.CheckExist("Episode");

                 

                    string result = await _playerMedia.GetAttachmentPermission(userId.Value, episodeId);

                    return new OkObjectResult(result);
                }
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(CourseResponse))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> OpenCourse(int courseId)
        {
            try
            {
                var userId = User.GetUserId();
         

                var activeSubscription = true;
                if (userId.HasValue)
                {
                    var user = await _userSvc.GetUserById(userId.Value);
                    user.CheckExist("User");
                    var result = await _subscriptionSvc.IsUserSubscriptionActive(user.Id);
                    if (!result)
                    {
                        bool isUserOwner = await _userSvc.IsUserOwnerOfCourse(user.Id, courseId);
                        if (!isUserOwner)
                        {
                            activeSubscription = false;
                            //throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                        }
                    }
                }

                var course = await _playerMedia.OpenCourse(userId, courseId, activeSubscription);
                return new OkObjectResult(course);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(EpisodeResponse))]
        //[AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> OpenEpisode(int episodeId)
        {
            try
            {
                EpisodeResponse episode = null;
                var userId = User.GetUserId();
                //if (!userId.HasValue)
                //{
                //    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                //}

                if (userId.HasValue)
                {
                    Console.WriteLine($"User ID is {userId}");
                    var user = await _userSvc.GetUserById(userId.Value);
                    user.CheckExist("User");

                    Console.WriteLine("OpenEpisode: user exists");

                    var activeSubscription = false;
                    var result = await _subscriptionSvc.IsUserSubscriptionActive(user.Id);
                    if (!result)
                    {
                        
                        bool isUserOwner = await _userSvc.IsUserOwnerOfEpisode(user.Id, episodeId);
                        if (isUserOwner)
                        {
                            activeSubscription = true;
                            //throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                        }
                    }
                    else
                    {
                        activeSubscription = true;
                    }
                    Console.WriteLine("User sub is active:"+activeSubscription.ToString());
                    episode = await _playerMedia.OpenEpisode(userId.Value, episodeId, activeSubscription);
                }
                else
                {
                    Console.WriteLine("User not authenticated");
                    episode = await _playerMedia.OpenEpisode(userId, episodeId, false);
                }


                return new OkObjectResult(episode);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(ICollection<EpisodeResponse>))]
        [HttpGet]
        public async Task<IActionResult> GetLastOpenedEpisodesInCourse(int courseId)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = _httpContextAccessor.HttpContext.User.GetUserRole();

                var user = await _userSvc.GetUserById(userId.Value);
                user.CheckExist("User");

                var result = await _subscriptionSvc.IsUserSubscriptionActive(user.Id);
                if (!result)
                {
                    throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                }

                var episodes = await _playerMedia.LastOpenedEpisodesInCourse(userId.Value, courseId);
                return new OkObjectResult(episodes);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(ICollection<CourseResponseWithWatchedStatus>))]
        [HttpGet]
        public async Task<IActionResult> GetLastOpenedCourses()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();
                
                var user = await _userSvc.GetUserById(userId.Value);
                user.CheckExist("User");

                //var result = await _subscriptionSvc.IsUserSubscriptionActive(user.Id);
                //if (!result)
                //{
                //    throw new CoachOnlineException("User does not have an active subscription.", CoachOnlineExceptionState.SubscriptionNotExist);
                //}

                var courses = await _playerMedia.LastOpenedCourses(userId.Value);
                return new OkObjectResult(courses);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [SwaggerResponse(200, Type = typeof(ICollection<CourseResponse>))]
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetLastAddedCourses()
        {
            try
            {
                var userId = User.GetUserId();

                var courses = await _playerMedia.LastAddedCourses(userId);
                return new OkObjectResult(courses);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPatch("/api/[controller]/course/{courseId}/like")]
        public async Task<IActionResult> LikeACourse(int courseId)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User does not exist", CoachOnlineExceptionState.NotExist);
                }

                var likesCnt = await _playerMedia.EvalCourse(courseId, userId.Value, true);

                return new OkObjectResult(new { LikesCnt = likesCnt });
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("/api/[controller]/course/{courseId}/unlike")]
        public async Task<IActionResult> UnlikeACourse(int courseId)
        {
            try
            {
                var userId = User.GetUserId();

                if(!userId.HasValue)
                {
                    throw new CoachOnlineException("User does not exist", CoachOnlineExceptionState.NotExist);
                }

                var likesCnt = await _playerMedia.EvalCourse(courseId, userId.Value, false);

                return new OkObjectResult(new { LikesCnt = likesCnt });

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost("/api/[controller]/course/{courseId}/comments")]
        public async Task<IActionResult> AddComment(int courseId, [FromBody]CommentRqs rqs)
        {
            try 
            {
                var userId = User.GetUserId();

                if(!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }
                var userRole = User.GetUserRole();

                if(userRole == UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("Wrong type of account.", CoachOnlineExceptionState.PermissionDenied);
                }


                await _commentSvc.AddComent(courseId, userId.Value, rqs.CommentTxt);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("/api/[controller]/course/{courseId}/comments/{commentId}/reply")]
        public async Task<IActionResult> ReplyToComment(int courseId, int commentId, [FromBody] CommentRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }
                var userRole = User.GetUserRole();

                if (userRole == UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("Wrong type of account.", CoachOnlineExceptionState.PermissionDenied);
                }


                await _commentSvc.ReplyToComment(commentId, courseId, userId.Value, rqs.CommentTxt);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpDelete("/api/[controller]/course/{courseId}/comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int courseId, int commentId, [FromBody] CommentRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }
                var userRole = User.GetUserRole();
                bool isAdmin = false;
                if (userRole == UserRoleType.ADMIN.ToString())
                {
                    isAdmin = true;
                }


                await _commentSvc.DeleteComment(commentId, courseId, userId.Value, isAdmin);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("/api/[controller]/course/{courseId}/comments/{commentId}")]
        public async Task<IActionResult> EditComment(int courseId, int commentId, [FromBody] CommentRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }
                var userRole = User.GetUserRole();

                if (userRole == UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("Wrong type of account.", CoachOnlineExceptionState.PermissionDenied);
                }


                await _commentSvc.EditComment(commentId, courseId, userId.Value, rqs.CommentTxt);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.CourseCommentResponse>))]
        [AllowAnonymous]
        [HttpGet("/api/[controller]/course/{courseId}/comments")]
        public async Task<IActionResult> GetComments(int courseId)
        {
            try
            {
                var userId = User.GetUserId();

                string userRole = "";
                if (userId.HasValue)
                { userRole = User.GetUserRole(); }

                var comments = await _commentSvc.GetCourseComments(courseId, userId, userRole == UserRoleType.ADMIN.ToString());
                return new OkObjectResult(comments);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}
