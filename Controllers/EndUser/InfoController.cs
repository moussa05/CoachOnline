using CoachOnline.Helpers;
using CoachOnline.Statics;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.EndUser
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        private readonly ILogger<InfoController> _logger;
        private readonly IUser _userSvc;
        private readonly IUserInfo _userInfoSvc;
        public InfoController(ILogger<InfoController> logger, IUser userSvc, IUserInfo userInfoSvc)
        {
            _logger = logger;
            _userSvc = userSvc;
            _userInfoSvc = userInfoSvc;
        }

        [HttpGet("{coachId}")]
        public async Task<IActionResult> Coach(int coachId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _userInfoSvc.GetCoachData(coachId);

                return new OkObjectResult(data);
                
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

        [HttpGet("{coachId}")]
        public async Task<IActionResult> CoachDocument(int coachId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _userInfoSvc.GetCoachDocumentData(coachId);

                return new OkObjectResult(data);
                
                
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

        [HttpGet("{courseId}")]
        public async Task<IActionResult> CourseInfo(int courseId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _userInfoSvc.GetCourseInfoData(courseId);

                return new OkObjectResult(data);
                
                
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

        [HttpGet("{courseId}")]
        public async Task<IActionResult> Course(int courseId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _userInfoSvc.GetCourse(courseId,userId.Value);

                return new OkObjectResult(data);

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
    }
}
