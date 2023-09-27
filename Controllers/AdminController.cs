using CoachOnline.ElasticSearch.Services;
using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using CoachOnline.Model.ApiResponses.Admin;
using CoachOnline.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Rollbar;
using Stripe;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers
{
    [Route("api/[controller]/[action]")]


    [ApiController]
    public class AdminController : ControllerBase
    {
        public AdminController(IAdmin _admin, ILogger<AdminController> _logger, IUser userSvc, ISubscription subscription, IHttpContextAccessor httpContextAccessor, ISearch searchSvc, IAffiliate affSvc, ICounter counterSvc)
        {
            this.admin = _admin;
            this.logger = _logger;
            this._userSvc = userSvc;
            _subscriptionSvc = subscription;
            _httpContextAccessor = httpContextAccessor;
            _searchSvc = searchSvc;
            _affSvc = affSvc;
            _counterSvc = counterSvc;
        }
        private readonly IAdmin admin;
        private readonly IAffiliate _affSvc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUser _userSvc;
        private readonly ISubscription _subscriptionSvc;
        private readonly ILogger<AdminController> logger;
        private readonly ISearch _searchSvc;
        private readonly ICounter _counterSvc;


        //[HttpGet("{s}")]
        //public IActionResult TestDiatrics(string s)
        //{
        //    try
        //    {
        //        var result = Extensions.RemoveDiacritics(s);
        //        return Ok(result);
        //    }
        //    catch(Exception ex)
        //    {
        //        return BadRequest();
        //    }
        //}
        //[HttpGet]
        //public async Task<ActionResult> TestConverting()
        //{
        //    try
        //    {

        //        await ConverterService.ConvertMachine();
        //        return Ok();

        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        Console.WriteLine(e);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
        //        Console.WriteLine(e);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<UploadPhotoResponse>> UploadCoursePhoto([FromBody] UploadCoursePhotoRequestAdmin request)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var response = await admin.UploadCoursePhoto(request.PhotoBase64, request.CourseId);

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

        [Authorize]
        [HttpPatch("/api/[controller]/courses/{courseId}/banner")]
        public async Task<ActionResult<UploadPhotoResponse>> UploadCourseBannerPhoto(int courseId, [FromBody] PhotoBase64Rqs request)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var response = await admin.UploadCourseBannerPhoto(request.ImgBase64, courseId);

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


        [Authorize]
        [HttpPatch("/api/[controller]/users/{userId}/password")]
        public async Task<ActionResult<UploadPhotoResponse>> ResetUserPassword(int userId, [FromBody] UpdateUserPasswordRqs request)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }


                await admin.UpdateUserPassword(userId, request.Password, request.RepeatPassword);

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
        public async Task<ActionResult<UpdateProfileAvatarResponse>> UpdateProfileAvatar([FromBody] UpdateCoachPhotoAsAdminRequest request)
        {
            try
            {

                UpdateProfileAvatarResponse response = new UpdateProfileAvatarResponse { FileName = "" };

                string file = await admin.UpdateCoachPhoto(request);
                response.FileName = file;
                return Ok(response);

            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<UpdateCoachCVResponse>> UpdateProfileCoachCV([FromBody] UpdateCoachCVAsAdminRequest request)
        {
            try
            {

                UpdateCoachCVResponse response = new UpdateCoachCVResponse { FileName = "" };

                string file = await admin.UpdateCoachCV(request);
                response.FileName = file;
                return Ok(response);

            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<UpdateCoachReturnsResponse>> UpdateProfileCoachReturns([FromBody] UpdateCoachReturnsAsAdminRequest request)
        {
            try
            {

                UpdateCoachReturnsResponse response = new UpdateCoachReturnsResponse { Returns = new List<string>() };

                List<string> filesName = await admin.UpdateCoachReturns(request);
                response.Returns = filesName;
                return Ok(response);

            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<UpdateCoachAttestationsResponse>> UpdateProfileCoachAttestations([FromBody] UpdateCoachAttestationAsAdminRequest request)
        {
            try
            {

                UpdateCoachAttestationsResponse response = new UpdateCoachAttestationsResponse { Diplomas = new List<string>() };

                List<string> filesName = await admin.UpdateCoachAttestations(request);
                response.Diplomas = filesName;
                return Ok(response);

            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCompanyInfo([FromBody] UpdateUserBillingInfoAsAdminRequest request)
        {
            try
            {
                await admin.UpdateUserBillingInfo(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPost]
        public async Task<ActionResult> UpdateUserData([FromBody] UpdateUserProfileAsAdminRequest request)
        {
            try
            {
                await admin.UpdateUserProfile(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [HttpPost]
        public async Task<ActionResult<GetAdminAuthTokenResponse>> GetAdminAuthToken([FromBody] GetAdminAuthTokenRequest request)
        {
            try
            {
                string token = await admin.GetAdminAuthToken(request.Email, request.Password);
                return Ok(new GetAdminAuthTokenResponse { AdminAuthToken = token });
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }

        }


        [HttpPost]
        public async Task<ActionResult> AssignChildToCategory([FromBody] AssignDismissChildRequest request)
        {
            try
            {
                await admin.AssignChildToCategory(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> DismissChildFromCategory([FromBody] AssignDismissChildRequest request)
        {
            try
            {
                await admin.DismissChildFromCategory(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCategoryFamily([FromBody] UpdateCategoryFamilyRequest request)
        {
            try
            {
                await admin.UpdateCategoryFamily(request);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetUserSubscriptionData([FromBody] GetUserRqs rqs)
        {
            try
            {
                var usr = await _userSvc.GetAdminByTokenAsync(rqs.AdminAuthToken);
                usr.CheckExist("User");
                var data = await admin.GetUserSubscriptionData(rqs.UserId);
                return new OkObjectResult(data);
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
        [HttpGet]
        public async Task<IActionResult> GetCategoriesSuggestedByUsers()
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await admin.GetCategoriesSuggestedByUsers();
                return new OkObjectResult(data);
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
        [HttpPut("{categoryId}")]
        public async Task<IActionResult> AcceptSuggestedCategory(int categoryId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                await admin.AcceptSuggestedCategory(categoryId);
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
        [HttpPost]
        public async Task<IActionResult> RejectSuggestedCategory([FromBody] RejectSuggestedCategoryRqs rqs)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                await admin.RejectSuggestedCategory(rqs.Id, rqs.RejectReason);
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
        [HttpPost]
        public async Task<IActionResult> RecreateSuggestedCourses([FromBody] ReSuggestCoursesRqs rqs)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                await _counterSvc.ReSuggestVideosForDay(rqs.ForDay.Date, rqs.CountFromMonths);
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
        [HttpDelete("{attachmentId}")]
        public async Task<IActionResult> DeleteEpisodeAttachment(int attachmentId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                await admin.DeleteAttachment(attachmentId);
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
        [HttpPatch("{episodeId}")]
        public async Task<IActionResult> AddEpisodeAttachment(int episodeId, [FromBody] AddAttachmentToEpisodeAdminRqs rqs)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                await admin.AddAttachment(episodeId,rqs.AttachmentName, rqs.AttachmentExtension, rqs.AttachmentBase64);
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
        [HttpDelete("{userId}")]
        public async Task<ActionResult> DeleteAccount(int userId)
        {
            try
            {
                var adminId = _httpContextAccessor.HttpContext.User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = _httpContextAccessor.HttpContext.User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var user = await _userSvc.GetUserById(userId);
                user.CheckExist("User");
                await _userSvc.DeleteAccount(user.Id);

                if (user.UserRole == UserRoleType.COACH)
                {
                    await _searchSvc.ReindexAll();
                }
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<GetUsersResponse>> GetUsers([FromBody] GetUsersRequest request)
        {

            try
            {
                GetUsersResponse response = null;
                if (request.FilterByRole)
                {
                    if (request.Role == "STUDENT")
                    {
                        response = await admin.GetUsers(request.AdminAuthToken, request.Count, request.Last, request.FromOldest, Model.UserRoleType.STUDENT);
                    }
                    else if(request.Role == "INSTITUTION_STUDENT")
                    {
                        response = await admin.GetUsers(request.AdminAuthToken, request.Count, request.Last, request.FromOldest, Model.UserRoleType.INSTITUTION_STUDENT);
                    }
                    else
                    {
                        response = await admin.GetUsers(request.AdminAuthToken, request.Count, request.Last, request.FromOldest, Model.UserRoleType.COACH);
                    }
                }
                else
                {
                    response = await admin.GetUsers(request.AdminAuthToken, request.Count, request.Last, request.FromOldest);
                }
                return Ok(response);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpGet("{user_id}")]
        public async Task<ActionResult<GetUsersResponse>> GetUser(int user_id)
        {

            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if(role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await admin.GetUserData(user_id);
                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [Authorize]
        [HttpGet("{course_id}")]
        public async Task<ActionResult<GetUsersResponse>> GetCourse(int course_id)
        {

            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await admin.GetCourse(course_id);
                return new OkObjectResult(resp);
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult<CreateCategoryAdminResponse>> CreateCategory([FromBody] CreateCategoryAdminRequest request)
        {
            try
            {
                int NewCategoryId = await admin.CreateCategory(request.AdminAuthToken, request.CategoryName);
                return Ok(new CreateCategoryAdminResponse
                {
                    NewCategoryId = NewCategoryId
                });


            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<ActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            try
            {
                await admin.UpdateCategoryName(request.AdminAuthToken, request.CategoryId, request.CategoryNewName);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }

        }

        [HttpPost]
        public async Task<ActionResult> RemoveCategory([FromBody] RemoveCategoryRequest request)
        {
            try
            {
                await admin.RemoveCategory(request.AdminAuthToken, request.CategoryId);
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
        public async Task<ActionResult<GetCoursesAsAdminResponse>> GetCourses([FromBody] GetCoursesAsAdminRequest request)
        {
            try
            {

                var response = await admin.GetCoursesWithUsers(request);
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
        public async Task<ActionResult> ChangeCourseState([FromBody] ChangeCourseStatusRequest request)
        {
            try
            {
                await admin.ChangeCourseState(request.AdminAuthToken, request.CourseId, request.CourseStatus);
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
        public async Task<ActionResult<GetPossibleCourseStatesResponse>> GetPossibleCourseStates()
        {
            try
            {
                GetPossibleCourseStatesResponse response = new GetPossibleCourseStatesResponse();
                response.States = new List<GetPossibleCourseStatesDate>();

                response.States.Add(new GetPossibleCourseStatesDate
                {
                    State = Model.CourseState.PENDING,
                    StateName = $"{Model.CourseState.PENDING}"
                });

                response.States.Add(new GetPossibleCourseStatesDate
                {
                    State = Model.CourseState.REJECTED,
                    StateName = $"{Model.CourseState.REJECTED}"
                });

                response.States.Add(new GetPossibleCourseStatesDate
                {
                    State = Model.CourseState.APPROVED,
                    StateName = $"{Model.CourseState.APPROVED}"
                });
                response.States.Add(new GetPossibleCourseStatesDate
                {
                    State = Model.CourseState.UNPUBLISHED,
                    StateName = $"{Model.CourseState.UNPUBLISHED}"
                });



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
        public async Task<ActionResult> AcceptCourse([FromBody] AcceptCourseRequest request)
        {
            try
            {
                await admin.AcceptCourse(request.AdminAuthToken, request.CourseId);
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
        public async Task<ActionResult> RejectCourse([FromBody] RejectCourseRequest request)
        {
            try
            {
                await admin.RejectCourse(request.AdminAuthToken, request.CourseId, request.Reason);
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
        public async Task<ActionResult> UpdateEpisodeInCourse([FromBody] UpdateAdminEpisodeInCourseRequest request)
        {
            try
            {
                await admin.UpdateEpisodeInCourse(request.AdminAuthToken, request.CourseId, request.EpisodeId, request.Title, request.Description, request.OrdinalNumber);
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
        public async Task<ActionResult> UpdateCourseDetails([FromBody] UpdateCourseAdminRequest request)
        {
            try
            {
                await admin.UpdateCourseDetailsAsync(request);
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
        public async Task<ActionResult> RemoveEpisodeFromCourse([FromBody] AdminRemoveLessonFromCourseRequest request)
        {
            try
            {
                await admin.RemoveEpisodeFromCourse(request.AdminAuthToken, request.CourseId, request.EpisodeId);
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
        public async Task<ActionResult> RemoveAttachmentFromEpisode([FromBody] AdminRemoveAttachment request)
        {
            try
            {
                await admin.RemoveAttachmentFromCourse(request.AdminAuthToken, request.CourseId, request.EpisodeId, request.AttachmentId);
                //todo
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
        public async Task<ActionResult> RemoveMediaFromEpisode([FromBody] AdminRemoveMedia request)
        {
            try
            {
                await admin.RemoveMediaFromEpisode(request.AdminAuthToken, request.CourseId, request.EpisodeId);
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
        public async Task<ActionResult<GetUserPossibleStatusesResponse>> GetUserPossibleStatuses()
        {
            try
            {
                GetUserPossibleStatusesResponse response = new GetUserPossibleStatusesResponse();
                response.Statuses = new List<ItemOfGetUserPossibleStatusesResponse>();

                //response.Statuses.Add(new ItemOfGetUserPossibleStatusesResponse
                //{
                //    Status = Model.UserAccountStatus.AWAITING_EMAIL_CONFIRMATION,
                //    StatusString = $"{Model.UserAccountStatus.AWAITING_EMAIL_CONFIRMATION}"
                //});


                response.Statuses.Add(new ItemOfGetUserPossibleStatusesResponse
                {
                    Status = Model.UserAccountStatus.BANNED,
                    StatusString = $"{Model.UserAccountStatus.BANNED}"
                });

                response.Statuses.Add(new ItemOfGetUserPossibleStatusesResponse
                {
                    Status = Model.UserAccountStatus.CONFIRMED,
                    StatusString = $"{Model.UserAccountStatus.CONFIRMED}"
                });

                return response;
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
        public async Task<ActionResult> ChangeUserStatus([FromBody] ChangeUserAccountStatusRequest request)
        {
            try
            {
                await admin.ChangeUserState(request.AdminAuthToken, request.UserId, request.NewStatus);
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
        public async Task<ActionResult> RemoveACourse([FromBody] AdminRemoveCourseRequest request)
        {
            try
            {
                await admin.RemoveCourse(request.AdminAuthToken, request.CourseId);
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
        public async Task<ActionResult<GetAdminCategoriesResponse>> GetCategories([FromBody] GetAdminCategoriesRequest request)
        {
            try
            {
                var response = await admin.GetCategories(request);
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

        [Authorize]
        [HttpPost("/api/[controller]/users/affiliates/extract")]
        public async Task<IActionResult> ExtractAffiliateHostsData([FromBody] TimeRangeRqs rqs)
        {
            try
            {

                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (myRole != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var response = await admin.GetAffiliatesData(rqs.Start, rqs.End);
                var excelResp = response.Select(x => new { x.Email, x.FirstName, x.LastName, Phone = x.PhoneNo, x.RegistrationDate, AffiliatorType=x.AffiliatorTypeStr, Plan = x.SubscriptionPlan, Role = x.UserType, Type = x.AffiliateType, Affiliation = x.IsAffiliate, Godfather = x.GodfatherName, GodfatherEmail = x.GodfatherEmail,
                    x.HostGodfatherName, x.HostGodfatherEmail,
                FirstLineAffiliatesQuantity=x.FirstLineAffiliatesQty, SecondLineAffiliatesQuantuty = x.SecondLineAffiliatesQty, TotalAffiliatesQuantity = x.TotalAffiliatesQty, TotalAffiliateIncome = x.TotalIncome, Currency = x.Currency, Origin = x.Origin}).ToList();
                
                if(!excelResp.Any())
                {
                    throw new CoachOnlineException("No data to export", CoachOnlineExceptionState.NotExist);
                }

                string resp = "";
                await Task.Run(() => {
                    resp = Helpers.Extensions.WriteToExcel(excelResp, "Affiliates");
                });

                string filename = "export_aff.xlsx";
                string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if(!System.IO.File.Exists(resp))
                {
                    throw new CoachOnlineException("File does not exists", CoachOnlineExceptionState.NotExist);
                }

                var memory = new System.IO.MemoryStream();
                
                    using (var stream = new System.IO.FileStream(resp, System.IO.FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    return File(memory, mime, filename);
                
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


        [Authorize(Roles ="ADMIN")]
        [HttpPost("/api/[controller]/users/affiliates")]
        public async Task<IActionResult> GetAffiliateHostsData([FromBody] TimeRangeRqs rqs)
        {
            try
            {

                var response = await admin.GetAffiliatesData(rqs.Start, rqs.End);
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

        [Authorize]
        [HttpPost("/api/[controller]/users/extract")]
        public async Task<IActionResult> ExtractUsersData([FromBody] UserTypeWithTimeRangeRqs rqs)
        {
            try
            {

                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var myRole = User.GetUserRole();

                if (myRole != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                var response = await admin.ExtractUserData(rqs.Role, rqs.Start, rqs.End);

                var excelResp = response.Select(x => new { x.Email, x.FirstName, x.LastName, Phone = x.PhoneNo, x.RegistrationDate, Plan = x.SubscriptionPlan, Role = x.UserType, Affiliation = x.IsAffiliate, Godfather=x.GodfatherName, GodfatherEmail = x.GodfatherEmail, x.HostGodfatherName, x.HostGodfatherEmail, x.Origin, x.LibraryName }).ToList();

                if (!excelResp.Any())
                {
                    throw new CoachOnlineException("No data to export", CoachOnlineExceptionState.NotExist);
                }

                string resp = "";
                await Task.Run(() => {
                    resp = Helpers.Extensions.WriteToExcel(excelResp, "Users");
                });
                
                string filename = "export.xlsx";
                string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if (!System.IO.File.Exists(resp))
                {
                    throw new CoachOnlineException("File does not exists", CoachOnlineExceptionState.NotExist);
                }

                var memory = new System.IO.MemoryStream();
                
                    using (var stream = new System.IO.FileStream(resp, System.IO.FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;

                    return File(memory, mime, filename);
                
                //return File($"{ConfigData.Config.SiteUrl}/tempfiles/{resp}", mime, filename);
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
        [HttpGet]
        public async Task<IActionResult> GetStudentCardsToAccept(int? status)
        {
            try
            {

                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }
                //var user = await _userSvc.GetAdminByTokenAsync(adminAuthToken);
                //user.CheckExist("Admin");

                var response = await _subscriptionSvc.AdminGetStudentCardsToAccept(status);
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
        public async Task<IActionResult> AcceptStudentCard([FromBody] AcceptStudentCardRqs rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AdminAuthToken);
                user.CheckExist("User");
                await _subscriptionSvc.AdminAcceptStudentCard(rqs.SubscriptionId);
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
        public async Task<IActionResult> RejectStudentCard([FromBody] RejectStudentCardRqs rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AdminAuthToken);
                user.CheckExist("User");
                await _subscriptionSvc.AdminRejectStudentCard(rqs.SubscriptionId, rqs.Reason);
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
        public async Task<IActionResult> FlagCoursesToDisplay([FromBody] CourseFlaggedModelRqs rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");

                await admin.FlagCourses(rqs.FlaggedCourses);
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
        public async Task<IActionResult> GetCoursesToFlag([FromBody]AuthTokenOnlyRequest rqs)
        {
            try
            {
                var user = await _userSvc.GetAdminByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");

                var data = await admin.GetCoursesToFlag();

                return new OkObjectResult(data);

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

        [SwaggerResponse(200, Type = typeof(AdminAffiliateInfoResp))]
        [Authorize(Roles ="ADMIN")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetAffiliateInfo(int userId)
        {
            try
            {
                AdminAffiliateInfoResp resp = new AdminAffiliateInfoResp();
                resp.UserId = userId;
           
                try
                {
                    var affLink = await _affSvc.GetMyAffiliateLink(userId);
                    resp.AffiliateLink = affLink;
                   
                }
                catch(CoachOnlineException ex)
                {
                    resp.AffiliateLink = "";
                }

                try
                {
                    var affLink = await _affSvc.GetMyAffiliateLinkForCoach(userId);
                    resp.AffiliateLinkForCoaches = affLink;

                }
                catch (CoachOnlineException ex)
                {
                    resp.AffiliateLinkForCoaches = "";
                }
                var affiliates = await _affSvc.GetMyAffiliates(userId);


                var earnedMoneyTotal = await _affSvc.GetEarnedMoneyfromAffiliatesGeneral(userId);

                resp.Affiliates = affiliates;
                resp.TotalEarnings = earnedMoneyTotal;

                return new OkObjectResult(resp);

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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostPaymentsAPI>))]
        [Authorize]
        [HttpPost("{userId}")]
        public async Task<IActionResult> GetAffiliateEarningsInPeriod(int userId, [FromBody] AffiliateEarnedMoneyDatesRqs rqs)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not an admin.", CoachOnlineExceptionState.NotAuthorized);
                }

                if(!rqs.StartDate.HasValue || !rqs.EndDate.HasValue)
                {
                    throw new CoachOnlineException("Period is not set", CoachOnlineExceptionState.DataNotValid);
                }

                var earningsInPeriod = await  _affSvc.GetEarnedMoneyfromAffiliatesBetweenDates(userId, rqs.StartDate.Value, rqs.EndDate.Value);

                return new OkObjectResult(earningsInPeriod);

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


        [SwaggerResponse(200, Type = typeof(List<Model.Student.PaymentMethodResponse>))]
        [Authorize(Roles ="ADMIN")]
        [HttpGet("/api/[controller]/users/{userId}/paymethods")]
        public async Task<IActionResult> GetUserPaymentMethods(int userId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var payMethods = await _subscriptionSvc.GetUserPaymentMethods(userId);

                return new OkObjectResult(payMethods);

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


        [Authorize(Roles ="ADMIN")]
        [HttpPatch("/api/[controller]/users/{userId}/paymethods/{payMethodId}")]
        public async Task<IActionResult> UpdateUserPaymentMethod(int userId, string payMethodId, [FromBody] PaymentMethodBillingDetailsOptions opts)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                await _subscriptionSvc.UpdateUserPaymentMethod(userId, payMethodId, opts);

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

        [Authorize(Roles ="ADMIN")]
        [HttpPatch("/api/[controller]/users/{userId}/paymethods/{payMethodId}/default")]
        public async Task<IActionResult> SetUserPaymentMethodDefault(int userId, string payMethodId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var user = await _userSvc.GetUserById(userId);
                user.CheckExist("User");
                await _subscriptionSvc.SetCustomerDefaultSource(user, payMethodId);

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

        [Authorize(Roles ="ADMIN")]
        [HttpDelete("/api/[controller]/users/{userId}/paymethods/{payMethodId}")]
        public async Task<IActionResult> DeleteUserPaymentMethod(int userId, string payMethodId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                await _subscriptionSvc.DeleteUserPaymentMethod(userId, payMethodId);

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

        [SwaggerResponse(200, Type = typeof(Model.Student.ClientSecretResponse))]
        [Authorize(Roles ="ADMIN")]
        [HttpPost("/api/[controller]/users/{userId}/paymethods")]
        public async Task<IActionResult> CreateUserPayMethod(int userId)
        {
            try
            {
                var adminId = User.GetUserId();
                if (!adminId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var user = await _userSvc.GetUserById(userId);
                user.CheckExist("User");

                var response = await _subscriptionSvc.CreateSetupIntent(user);

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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.Admin.SubCancellationReasonResponse>))]
        [Authorize(Roles ="ADMIN")]
        [HttpGet("/api/[controller]/subscriptions/cancelled/reasons")]
        public async Task<IActionResult> GetSubscriptionCancellationReasons()
        {
            try
            {
                var data = await _subscriptionSvc.GetSubscriptionCancellationReasons();

                return new OkObjectResult(data);
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

        //[HttpPost]
        //public async Task<ActionResult> UpdateEpisodeMedia([FromBody] UpdateAdminEpisodeMediaRequest request)
        //{
        //    try
        //    {
        //        await admin.UpdateEpisodeMedia(request.AuthToken, request.CourseId, request.EpisodeID);
        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

        //[HttpPost]
        //public async Task<ActionResult> UpdateEpisodeAttachment([FromBody] UpdateAdminEpisodeAttachmentRequest request)
        //{
        //    try
        //    {
        //        await admin.UpdateEpisodeAttachment(request.AdminAuthToken, request.CourseId, request.EpisodeID, request.AttachmentHash);
        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

    }
}
