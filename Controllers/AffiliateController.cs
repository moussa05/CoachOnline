using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.ApiRequests.Admin;
using CoachOnline.Model.ApiRequests.B2B;
using CoachOnline.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.AffiliateService;

namespace CoachOnline.Controllers
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AffiliateController : ControllerBase
    {
        ILogger<AffiliateController> _logger;
        private readonly IAffiliate _affSvc;
        private readonly IRequestedPayments _reqPayments;

        public AffiliateController(ILogger<AffiliateController> logger, IAffiliate affiliateSvc, IRequestedPayments reqPayments)
        {
            _logger = logger;
            _affSvc = affiliateSvc;
            _reqPayments = reqPayments;
        }


        [Authorize(Roles = "ADMIN")]
        [HttpPatch("/api/[controller]/users/{userId}/affType/{affType}")]
        public async Task<IActionResult> ChangeAffiliationModelForUser(int userId, AffiliateModelType affType)
        {
            try
            {
                await _affSvc.ChangeAffiliationModelForUser(userId, affType);
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

        [HttpPost]
        public async Task<IActionResult> GenerateAffiliateLink()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                string token = await _affSvc.GenerateAffiliateLink(userId.Value);
                return new OkObjectResult(new { Link = token });
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




        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostsRankingResponse>))]
        [HttpGet("/api/[controller]/ranking/{type}")]
        public async Task<IActionResult> GetHostsRanking(HostsRankingType type)
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (!currentUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                int? userId = null;

                if (role == UserRoleType.ADMIN.ToString())
                {
                    userId = null;
                }
                else if (role == UserRoleType.STUDENT.ToString() || role == UserRoleType.COACH.ToString())
                {
                    userId = currentUserId.Value;
                }
                else
                {
                    throw new CoachOnlineException("User has no access to this data.", CoachOnlineExceptionState.PermissionDenied);
                }

                var data = await _affSvc.GetAffiliateHostsRanking(type, false, userId);
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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostsRankingResponse>))]

        [HttpGet("/api/[controller]/ranking/{type}/top")]
        public async Task<IActionResult> GetHostsRankingTop10(HostsRankingType type)
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (!currentUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                int? userId = null;

                if (role == UserRoleType.ADMIN.ToString())
                {
                    userId = null;
                }
                else if (role == UserRoleType.STUDENT.ToString() || role == UserRoleType.COACH.ToString())
                {
                    userId = currentUserId.Value;
                }
                else
                {
                    throw new CoachOnlineException("User has no access to this data.", CoachOnlineExceptionState.PermissionDenied);
                }

                var data = await _affSvc.GetAffiliateHostsRanking(type, true, userId);
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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostsRankingPagesResponse>))]

        [HttpGet("/api/[controller]/ranking/{type}/pages/{pageNo}")]
        public async Task<IActionResult> GetHostsRankingTop10(HostsRankingType type, int pageNo)
        {
            try
            {
                var currentUserId = User.GetUserId();
                if (!currentUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                int? userId = null;

                if (role == UserRoleType.ADMIN.ToString())
                {
                    userId = null;
                }
                else if (role == UserRoleType.STUDENT.ToString() || role == UserRoleType.COACH.ToString())
                {
                    userId = currentUserId.Value;
                }
                else
                {
                    throw new CoachOnlineException("User has no access to this data.", CoachOnlineExceptionState.PermissionDenied);
                }

                var data = await _affSvc.GetAffiliateHostsRanking(type, pageNo, userId);
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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffilationStatisticsResponse>))]

        [HttpGet("/api/[controller]/statistics")]
        public async Task<IActionResult> GetAffiliateAdminStats()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                var data = await _affSvc.GetAffiliateStats();
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



        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPost("/api/[controller]/users/{userId}/link")]
        public async Task<IActionResult> GenerateAffiliateLink(int userId)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                string token = await _affSvc.GenerateAffiliateLink(userId);
                return new OkObjectResult(new { Link = token });
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

        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPost("/api/[controller]/users/{userId}/coachlink")]
        public async Task<IActionResult> GenerateAffiliateLinkForCoaches(int userId)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                string token = await _affSvc.GenerateAffiliateLinkForCoach(userId);
                return new OkObjectResult(new { Link = token });
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
        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPatch("/api/[controller]/link/{proposal}")]
        public async Task<IActionResult> ProposeAffiliateLink(string proposal)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                string token = await _affSvc.ProposeAffiliateLink(userId.Value, proposal);
                return new OkObjectResult(new { Link = token });
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
        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPatch("/api/[controller]/coachlink/{proposal}")]
        public async Task<IActionResult> ProposeAffiliateLinkForCoach(string proposal)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                string token = await _affSvc.ProposeAffiliateLinkForCoach(userId.Value, proposal);
                return new OkObjectResult(new { Link = token });
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
        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPatch("/api/[controller]/users/{userId}/link/{proposal}")]
        public async Task<IActionResult> ProposeAffiliateLink(int userId, string proposal)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                string token = await _affSvc.ProposeAffiliateLink(userId, proposal);
                return new OkObjectResult(new { Link = token });
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

        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpPatch("/api/[controller]/users/{userId}/coachlink/{proposal}")]
        public async Task<IActionResult> ProposeAffiliateLinkForCoach(int userId, string proposal)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                string token = await _affSvc.ProposeAffiliateLinkForCoach(userId, proposal);
                return new OkObjectResult(new { Link = token });
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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.Admin.CouponResponse>))]
        [HttpGet("/api/[controller]/users/{userId}/coupons")]
        public async Task<IActionResult> GetAvailableCouponsForUser(int userId)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _affSvc.GetAvailableCouponsForUser(userId);
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

        [HttpPut("/api/[controller]/users/{userId}/link/{link}")]
        public async Task<IActionResult> UpdateAffiliateLink(int userId, string link, [FromBody] LinkUpdateOptionsRqs rqs)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                await _affSvc.UpdateAffiliateLinkOptions(userId, link, rqs);
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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.LinkOptsResponse>))]
        [HttpGet("/api/[controller]/user/{userId}/link/{link}/options")]
        public async Task<IActionResult> GetAffiliateLinkOptions(int userId, string link)
        {
            try
            {
                var loggedInUserId = User.GetUserId();
                if (!loggedInUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    //its ok
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (loggedInUserId.Value != userId && role != UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("User is not allowed to change other account link", CoachOnlineExceptionState.NotAuthorized);
                }

                var resp = await _affSvc.GetAffiliateLinkWithOptions(userId, link);
                return new OkObjectResult(resp);
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

        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpGet]
        public async Task<IActionResult> GetMyAffiliateLink()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                string token = await _affSvc.GetMyAffiliateLink(userId.Value);
                return new OkObjectResult(new { Link = token });
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



        [SwaggerResponse(200, Type = typeof(LinkHelper))]
        [HttpGet("/api/[controller]/users/{userId}/coachlink")]
        public async Task<IActionResult> GetMyAffiliateLinkForCoach(int userId)
        {
            try
            {
                int idOfUser = -1;
                var userID = User.GetUserId();
                if (!userID.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (role == UserRoleType.ADMIN.ToString())
                {
                    idOfUser = userId;
                }
                else if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }
                else
                {
                    idOfUser = userID.Value;
                }



                var token = await _affSvc.GetMyAffiliateLinkForCoach(idOfUser);
                return new OkObjectResult(new { Link = token });
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

        [AllowAnonymous]
        [HttpGet("/ref/{link}")]
        public async Task<IActionResult> Ref(string link)
        {
            try
            {
                string url = $"{Statics.ConfigData.Config.WebUrl}";
                var token = await _affSvc.GetTokenByAffLink(link);

                var retUrl = $"{Statics.ConfigData.Config.WebUrl}/";

                if (!string.IsNullOrWhiteSpace(token.ReturnUrl))
                {
                    retUrl = token.ReturnUrl;
                }

                if (token.ForCoach)
                {

                    url = $"{retUrl}?Ref={token.GeneratedToken}";
                }
                else
                {
                    url = $"{retUrl}?Join={token.GeneratedToken}";
                }

                return Redirect(url);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}");
            }
        }

        [AllowAnonymous]
        [HttpGet("/api/[controller]/ref/{link}")]
        public async Task<IActionResult> RefOld(string link)
        {
            try
            {
                var token = await _affSvc.GetTokenByAffLink(link);

                var url = $"{Statics.ConfigData.Config.WebUrl}/?Join={token.GeneratedToken}";


                return Redirect(url);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return Redirect($"{Statics.ConfigData.Config.WebUrl}");
            }
        }

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateAPI>))]
        [HttpGet]
        public async Task<IActionResult> GetMyAffiliates()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                var affiliates = await _affSvc.GetMyAffiliates(userId.Value);
                return new OkObjectResult(affiliates);
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


        [Authorize]
        [HttpPost("/api/[controller]/users/{userId}/affiliates/extract")]
        public async Task<IActionResult> ExtractAffiliateHostsData(int userId)
        {
            try
            {

                var authUserId = User.GetUserId();
                if (!authUserId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                if (authUserId.Value != userId)
                {
                    throw new CoachOnlineException("Permission Denied", CoachOnlineExceptionState.PermissionDenied);
                }



                var response = await _affSvc.GetMyAffiliates(userId);
                var excelResp = response.Select(x => new
                {
                    x.Email,
                    x.FirstName,
                    x.LastName,
                    Plan = x.ChosenPlan ?? "",
                    Role = x.UserRole ?? "",
                    Type = x.Type ?? "",
                    Godfather = x.Host == null ? "" : x.Host.FirstName?.ToString() + " " + x.Host.LastName?.ToString(),
                    GodfatherEmail = x.Host.Email,
                    FirstLineAffiliatesQuantity = x.Affiliates == null ? 0 : x.Affiliates.Where(x => x.IsDirect).Count(),
                    SecondLineAffiliatesQuantuty = x.Affiliates == null ? 0 : x.Affiliates.Where(x => !x.IsDirect).Count(),
                    TotalAffiliatesQuantity = x.Affiliates == null ? 0 : x.Affiliates.Count,
                    TotalAffiliateIncome = x.EarnedMoney,
                    Currency = x.Currency ?? ""
                }).ToList();



                if (!excelResp.Any())
                {
                    throw new CoachOnlineException("No data to export", CoachOnlineExceptionState.NotExist);
                }

                string resp = "";
                await Task.Run(() =>
                {
                    resp = Helpers.Extensions.WriteToExcel(excelResp, "Affiliates");
                });

                string filename = "export_aff.xlsx";
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

        [SwaggerResponse(200, Type = typeof(List<Model.HelperModels.AffiliateSubscriptionStatus>))]
        [HttpGet]
        public async Task<IActionResult> CheckUserSubscriptionState()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                var affiliates = await _affSvc.CheckUserSubscription(userId.Value);
                return new OkObjectResult(affiliates);
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

        [HttpPost]
        public async Task<IActionResult> SendAffiliateInvitation([FromBody] SendAffiliateInvitationRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                await _affSvc.SendAffiliateEmailInvitation(userId.Value, rqs.AffiliateEmail);

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

        [HttpPost]
        public async Task<IActionResult> WithdrawMoney()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                await _affSvc.WithdrawPaymentByPaypal(userId.Value);

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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.RequestedPaymentResponse>))]
        [HttpGet("/api/[controller]/withdrawals/user/{userId}")]
        public async Task<IActionResult> GetUserWithdrawals(int userId)
        {
            try
            {
                var loggedInUser = User.GetUserId();
                if (!loggedInUser.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString() || role == UserRoleType.ADMIN.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if(role != UserRoleType.ADMIN.ToString() && loggedInUser.Value != userId)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _reqPayments.GetPaypalWithdrawalRequestedPaymentsForUser(userId);

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

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("/api/[controller]/withdrawals/{withdrawalId}/accept")]
        public async Task<IActionResult> AcceptWithdrawal(int withdrawalId)
        {
            try
            {
                await _reqPayments.AcceptPayPalWithdrawal(withdrawalId);

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


        [Authorize(Roles = "ADMIN")]
        [HttpPatch("/api/[controller]/withdrawals/{withdrawalId}/reject")]
        public async Task<IActionResult> RejectWithdrawal(int withdrawalId, [FromBody]WithdrawalRejectRqs rqs)
        {
            try
            {
                await _reqPayments.RejectPaypalWithdrawal(withdrawalId, rqs.RejectReason);

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


        [Authorize(Roles ="ADMIN")]
        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.RequestedPaymentResponse>))]
        [HttpGet("/api/[controller]/withdrawals")]
        public async Task<IActionResult> GetAllWithdrawals(RequestedPaymentStatus? status)
        {
            try
            {
                var data = await _reqPayments.GetPaypalWithdrawalRequestedPayments(status);

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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostPaymentsAPI>))]
        [HttpPost]
        public async Task<IActionResult> GetEarnedMoneyForPeriod([FromBody] AffiliateEarnedMoneyDatesRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (!rqs.EndDate.HasValue || !rqs.StartDate.HasValue)
                {
                    throw new CoachOnlineException("Dates are not selected", CoachOnlineExceptionState.DataNotValid);
                }

                var affiliates = await _affSvc.GetEarnedMoneyfromAffiliatesBetweenDates(userId.Value, rqs.StartDate.Value, rqs.EndDate.Value);
                return new OkObjectResult(affiliates);
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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostPaymentsAPI>))]
        [HttpGet]
        public async Task<IActionResult> GetEarnedMoneyForMonth(int? month, int? year)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                if (!month.HasValue || !year.HasValue)
                {
                    throw new CoachOnlineException("Month and year not selected", CoachOnlineExceptionState.DataNotValid);
                }

                var affiliates = await _affSvc.GetEarnedMoneyfromAffiliatesForMonth(userId.Value, month.Value, year.Value);
                return new OkObjectResult(affiliates);
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


        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.AffiliateHostPaymentsAPI>))]
        [HttpGet]
        public async Task<IActionResult> GetEarnedMoneyGeneral()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();

                if (!(role == UserRoleType.COACH.ToString() || role == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type", CoachOnlineExceptionState.PermissionDenied);
                }

                var affiliates = await _affSvc.GetEarnedMoneyfromAffiliatesGeneral(userId.Value);
                return new OkObjectResult(affiliates);
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
    public class LinkHelper
    {
        public string Link { get; set; }
    }
}
