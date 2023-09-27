using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using ITSAuth.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CoachOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {

        public CoursesController(ILogger<CoursesController> logger, ICoachService dataImpl, IEmailApiService _emailService)
        {
            this.logger = logger;
            this.dataImpl = dataImpl;
            this.emailService = _emailService;

        }
        private readonly IEmailApiService emailService;
        private readonly ILogger logger;
        private readonly ICoachService dataImpl;



        [HttpPost]
        public async Task<ActionResult> AssingCategoryToUser([FromBody] CategoryToUserRequest request)
        {
            try
            {
                await dataImpl.AssignCategoryToUser(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }

        }


        [Authorize]
        [HttpPatch("/api/[controller]/users/{userId}/categories/{categoryId}")]
        public async Task<ActionResult> AssingUserCategory(int userId, int categoryId)
        {
            try
            {
                var authId = User.GetUserId();
                if (!authId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (myRole == Model.UserRoleType.ADMIN.ToString())
                {
                    await dataImpl.AssignUserCategory(userId, categoryId);
                    return Ok();
                }
                else if (userId != authId.Value)
                {
                    throw new CoachOnlineException("Access denied", CoachOnlineExceptionState.NotAuthorized);
                }


                await dataImpl.AssignUserCategory(userId, categoryId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }

        }

        [Authorize]
        [HttpDelete("/api/[controller]/users/{userId}/categories/{categoryId}")]
        public async Task<ActionResult> DetachUserCategory(int userId, int categoryId)
        {
            try
            {

                var authId = User.GetUserId();
                if (!authId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (myRole == Model.UserRoleType.ADMIN.ToString())
                {
                    await dataImpl.DetachUserCategory(userId, categoryId);
                    return Ok();
                }
                else if (userId != authId.Value)
                {
                    throw new CoachOnlineException("Access denied", CoachOnlineExceptionState.NotAuthorized);
                }

                await dataImpl.DetachUserCategory(userId, categoryId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }


        }


        [HttpPost]
        public async Task<ActionResult> DetachCategoryFromUser([FromBody] CategoryToUserRequest request)
        {
            try
            {
                await dataImpl.DetachCategoryFromUser(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }


        }




        [SwaggerResponse(200, Type = typeof(CreateCourseResponse))]
        [HttpPost]
        public async Task<ActionResult<CreateCourseResponse>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            try
            {

                if (!request.Category.HasValue)
                {
                    logger.LogInformation("You have to select category for the course.");
                    throw new CoachOnlineException("Vous devez sélectionner une catégorie pour le cours", CoachOnlineExceptionState.DataNotValid);
                }
                int courseId = await dataImpl.CreateCourseAsync(request.AuthToken, request.Name, request.Category.Value, request.Description, request.PhotoUrl);
                return Ok(new CreateCourseResponse { CourseId = courseId });
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCourse([FromBody] UpdateCourseRequest request)
        {
            try
            {
                await dataImpl.UpdateCourseDetailsAsync(request.AuthToken, request.CourseId, request.Name, request.Category, request.Description, request.PhotoUrl);
                return Ok();
            }

            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPatch("/api/[controller]/courses/{courseId}/block")]
        public async Task<IActionResult> BlockCourse(int courseId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (!(myRole == Model.UserRoleType.COACH.ToString() || myRole == Model.UserRoleType.ADMIN.ToString()))
                {
                    throw new CoachOnlineException("Access denied", CoachOnlineExceptionState.NotAuthorized);
                }
                await dataImpl.BlockCourse(userId.Value, courseId, myRole);
                return Ok();
            }

            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpPatch("/api/[controller]/courses/{courseId}/unblock")]
        public async Task<IActionResult> UnBlockCourse(int courseId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (!(myRole == Model.UserRoleType.COACH.ToString() || myRole == Model.UserRoleType.ADMIN.ToString()))
                {
                    throw new CoachOnlineException("Access denied", CoachOnlineExceptionState.NotAuthorized);
                }
                await dataImpl.UnBlockCourse(userId.Value, courseId, myRole);
                return Ok();
            }

            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveCourse([FromBody] RemoveCourseRequest request)
        {
            try
            {
                await dataImpl.RemoveCourseAsync(request.AuthToken, request.CourseId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [SwaggerResponse(200, Type = typeof(AddEpisodeToCourseResponse))]

        [HttpPost]
        public async Task<ActionResult<AddEpisodeToCourseResponse>> AddEpisodeToCourse([FromBody] AddEpisodeToCourseRequest request)
        {
            try
            {
                int EpisodeId = await dataImpl.AddEpisodeToCourse(request.AuthToken, request.CourseId, request.Title, request.Description);
                return Ok(new AddEpisodeToCourseResponse { EpisodeId = EpisodeId });
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddEpisodeToCourseResponse>> AddPromoEpisodeToCourse([FromBody] AddEpisodeToCourseRequest request)
        {
            try
            {
                int EpisodeId = await dataImpl.AddPromoEpisodeToCourse(request.AuthToken, request.CourseId, request.Title, request.Description);
                return Ok(new AddEpisodeToCourseResponse { EpisodeId = EpisodeId });
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateEpisodeInCourse([FromBody] UpdateEpisodeInCourseRequest request)
        {
            try
            {
                await dataImpl.UpdateEpisodeInCourse(request.AuthToken, request.CourseId, request.EpisodeId, request.Title, request.Description, request.OrdinalNumber);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveEpisodeFromCourse([FromBody] RemoveEpisodeFromCourseRequest request)
        {
            try
            {
                await dataImpl.RemoveEpisodeFromCourse(request.AuthToken, request.CourseId, request.EpisodeId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateEpisodeMedia([FromBody] UpdateEpisodeMediaRequest request)
        {
            try
            {
                await dataImpl.UpdateEpisodeMedia(request.AuthToken, request.CourseId, request.EpisodeID);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateEpisodeAttachment([FromBody] UpdateEpisodeAttachmentRequest request)
        {
            try
            {
                //await dataImpl.UpdateEpisodeAttachment(request.AuthToken, request.CourseId, request.EpisodeID, request.AttachmentHash);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                await dataImpl.CreateCategoryAsync(request.Name, request.AuthToken);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost]
        public async Task<ActionResult<List<CourseResponse>>> GetCoursesForUser([FromBody] OnlyAuthTokenRequest request)
        {
            try
            {
                var courses = await dataImpl.GetCoursesForOwnerAsync(request.AuthToken);
                return Ok(courses);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<UploadPhotoResponse>> UploadPhoto([FromBody] UploadPhotoRequest request)
        {
            try
            {
                UploadPhotoResponse response = new UploadPhotoResponse();

                response.PhotoPath = await dataImpl.UploadPhoto(request.AuthToken, request.Base64Photo);

                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<UploadPhotoResponse>> UploadCoursePhoto([FromBody] UploadCoursePhotoRequest request)
        {
            try
            {
                var response = await dataImpl.UploadCoursePhoto(request.AuthToken, request.PhotoBase64, request.CourseId);

                return new OkObjectResult(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddAtachmentToEpisodeResponse>> AddAttachmentToEpisode([FromBody] AddAtachmentToEpisodeRequest request)
        {
            try
            {
                List<EpisodeAttachment> Attachments = await dataImpl.AddAttachmentToEpisode(request.AuthToken, request.AttachmentBase64, request.CourseId, request.EpisodeId, request.AttachmentExtension, request.AttachmentName);
                AddAtachmentToEpisodeResponse response = new AddAtachmentToEpisodeResponse();
                response.attachments = Attachments;

                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveAttachmentFromEpisode([FromBody] RemoveAttachmentFromEpisodeRequest request)
        {
            try
            {
                await dataImpl.RemoveAttachmentFromEpisode(request.AuthToken, request.CourseId, request.EpisodeId, request.AttachmentId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetCoursesCategoriesResponse>> GetCategories()
        {
            try
            {
                var response = await dataImpl.GetCategories();
                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<ActionResult<GetCoursesCategoriesResponse>> GetCategoriesCompleted()
        {
            try
            {
                var response = await dataImpl.GetCategoriesCompleted();
                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> RemoveMediaFromEpisode([FromBody] RemoveMediaFromEpisodeRequest request)
        {
            try
            {
                await dataImpl.RemoveMediaFromEpisode(request.AuthToken, request.CourseId, request.EpisodeId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost]
        public async Task<ActionResult> SubmitToVerification([FromBody] SubmitCourseRequest request)
        {
            try
            {
                await dataImpl.SubmitCourse(request.AuthToken, request.CourseId);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }






    }
}
