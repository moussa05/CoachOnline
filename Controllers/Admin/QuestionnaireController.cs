using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
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
using static CoachOnline.Services.QuestionnaireService;

namespace CoachOnline.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuestionnaireController : ControllerBase
    {
        private readonly ILogger<QuestionnaireController> _logger;
        private readonly IQuestionnaire _questionSvc;
        public QuestionnaireController(ILogger<QuestionnaireController> logger, IQuestionnaire questionSvc)
        {
            _logger = logger;
            _questionSvc = questionSvc;
        }

        [Authorize(Roles ="ADMIN")]
        [HttpPost("form")]
        public async Task<IActionResult> AddNewQuestionnaire([FromBody]QuestionnaireRqs rqs)
        {
            try
            {
                var id = await _questionSvc.AddForm(rqs);
                return new OkObjectResult(new { Id = id });
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
        [HttpPatch("form/{qId}")]
        public async Task<IActionResult> EditQuestionnaire(int qId, [FromBody] QuestionnaireRqs rqs)
        {
            try
            {
                var id = await _questionSvc.EditForm(qId, rqs);
                return new OkObjectResult(new { Id = id });
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

        [SwaggerResponse(200, Type = typeof(List<Model.ApiResponses.Admin.QuestionaaireStatsResponse>))]
        [Authorize(Roles = "ADMIN")]
        [HttpPatch("form/statistics")]
        public async Task<IActionResult> GetStats(QuestionnaireType? questionaireType)
        {
            try
            {
                var stats = await _questionSvc.GetStats(questionaireType);
                return new OkObjectResult(stats);
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

        [HttpGet("form/{type}")]
        public async Task<IActionResult> GetQuestionnaire(QuestionnaireType type)
        {
            try
            {
                var form = await _questionSvc.GetForm(type);
                return new OkObjectResult(form);
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

        [HttpGet("form")]
        public async Task<IActionResult> GetQuestionnaire()
        {
            try
            {
                var form = await _questionSvc.GetForm();
                return new OkObjectResult(form);
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

        [HttpPatch("form/{qId}/answer")]
        public async Task<IActionResult> AnswerQuestionnaire(int qId, [FromBody] QuestionnaireAnswerRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                var role = User.GetUserRole();


                if (role == UserRoleType.ADMIN.ToString())
                {
                    throw new CoachOnlineException("Wrong account type.", CoachOnlineExceptionState.PermissionDenied);
                }
                var answerId = await _questionSvc.RespondToForm(qId, userId.Value, rqs);
                return new OkObjectResult(new { AnswerId = answerId });
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
