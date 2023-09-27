using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Coach;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.Coach
{
    [Authorize]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly ILogger<EventController> _logger;
        private readonly IUser _userSvc;
        private readonly IEvent _evtSvc;
        public EventController(ILogger<EventController> logger, IUser userSvc, IEvent evtSvc)
        {
            _userSvc = userSvc;
            _logger = logger;
            _evtSvc = evtSvc;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEventDraft()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var eventId = await _evtSvc.CreateEventDraft(userId.Value);

                return new OkObjectResult(new { EventId = eventId });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{eventId}")]
        public async Task<IActionResult> UpdateEventDraftPage1(int eventId, [FromBody] UpdateEventRqs evt)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var evId = await _evtSvc.UpdateEventDraft(userId.Value, eventId, evt.Name, evt.Description, evt.CoverPictrueBase64, evt.StartDate, evt.EndDate, evt.Categories);

                return new OkObjectResult(new { EventId = evId });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{eventId}")]
        public async Task<IActionResult> UpdateEventDraftPage2(int eventId, [FromBody] UpdateEventRqs evt)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var evtId = await _evtSvc.UpdateEventDraft(userId.Value, eventId, evt.PersonQty, evt.Price, evt.Currency, evt.Attachments);

                return new OkObjectResult(new { EventId = evtId });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("{eventId}")]
        public async Task<IActionResult> CreateEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                await _evtSvc.CreateEvent(eventId, userId.Value);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpPatch("{eventId}")]
        public async Task<IActionResult> UpdateEventFields(int eventId, [FromBody] List<UpdateFieldRqs> rqsList)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                foreach (var rqs in rqsList)
                {
                    await _evtSvc.UpdateEventField(eventId, userId.Value, rqs.PropertyName, rqs.PropertyValue);
                }
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [HttpPatch("{eventId}")]
        public async Task<IActionResult> UpdateEventDates(int eventId, [FromBody] UpdateEventDatesRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                await _evtSvc.UpdateEventDates(userId.Value, eventId, rqs.StartDate, rqs.EndDate);
                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetMyEvents()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var events = await _evtSvc.GetUserEvents(userId.Value);

                return new OkObjectResult(events);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [HttpGet("{eventId}")]
        public async Task<IActionResult> GetMyEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }
                _logger.LogInformation($"Getting event {eventId}");
                var events = await _evtSvc.GetUserEvent(eventId, userId.Value);

                return new OkObjectResult(events);
            }
            catch (CoachOnlineException e)
            {
                _logger.LogInformation($"{e}");
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"{e}");

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetIncomingEvents()
        {
            try
            {


                var events = await _evtSvc.GetIncomingEvents();

                return new OkObjectResult(events);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserCreditCards()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var cards = await _evtSvc.GetPaymentMethods(userId.Value);

                return new OkObjectResult(cards);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost("{eventId}")]
        public async Task<IActionResult> AssignForEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var assId = await _evtSvc.AssignForEvent(userId.Value, eventId);

                return new OkObjectResult(new { AssignationId = assId });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> PayForEvent([FromBody] PayForEventRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                await _evtSvc.PayForEvent(userId.Value, rqs.AssignationId, rqs.PaymentMethodId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> RefundPaymentForEvent([FromBody] RefundEvtRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                await _evtSvc.RefundCustomerPaymentForEvent(userId.Value, rqs.AssignationId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("eventId")]
        public async Task<IActionResult> StartMyLiveEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                var liveToken = await _evtSvc.StartMyLiveEvent(eventId, userId.Value);

                return new OkObjectResult(new { LiveToken = liveToken });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPatch("eventId")]
        public async Task<IActionResult> JoinLiveEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var liveToken = await _evtSvc.JoinLiveEvent(userId.Value, eventId);

                return new OkObjectResult(new { LiveToken = liveToken });
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpDelete("eventId")]
        public async Task<IActionResult> DeleteMyEvent(int eventId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (role != Model.UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach.", CoachOnlineExceptionState.NotAuthorized);
                }

                await _evtSvc.DeleteMyEvent(userId.Value, eventId);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }


    }
}
