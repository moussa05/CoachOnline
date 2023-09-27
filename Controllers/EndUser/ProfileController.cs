using CoachOnline.Helpers;
using CoachOnline.Implementation;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rollbar;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.EndUser
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {

        ILogger<ProfileController> _logger;
        private readonly IUser _userSvc;
        private readonly ISubscription _subscriptionSvc;
        private readonly ICoachService _dataImpl;

        public ProfileController(ILogger<ProfileController> logger, IUser userSvc, ISubscription subscSvc, IServiceProvider _svcProvider)
        {
            _logger = logger;
            _userSvc = userSvc;
            _subscriptionSvc = subscSvc;
            _dataImpl = _svcProvider.GetService(typeof(ICoachService)) as ICoachService;
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserEmail([FromBody] UpdateUerEmailRqs request)
        {

            try
            {
                var user = await _userSvc.GetUserByTokenAsync(request.AuthToken);
                user.CheckExist("User");
                await _userSvc.UpdateUserEmail(user.Id, request.Email);
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

        [HttpGet]
        public async Task<IActionResult> ConfirmUserEmail(string Token)
        {

            try
            {
                var roleType = await _userSvc.ConfirmEmailUpdate(Token);
                if (roleType == Model.UserRoleType.COACH)
                {
                    return Redirect($"{Statics.ConfigData.Config.WebUrl}/profile?email_confirmed=true");
                }
                else
                {
                    return Redirect($"{Statics.ConfigData.Config.WebUrl}/studentProfile?email_confirmed=true");
                }
            }
            catch (CoachOnlineException e)
            {
                return Redirect($"{Statics.ConfigData.Config.WebUrl}?email_confirmed=false&reason={e.Message}");
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return Redirect($"{Statics.ConfigData.Config.WebUrl}?email_confirmed=false&reason=unknown");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateEndUserProfileRqs request)
        {

            try
            {
                var user = await _userSvc.GetUserByTokenAsync(request.AuthToken);
                user.CheckExist("User");
                await _userSvc.UpdateBasicUserData(user, request.Name, request.Surname, request.YearOfBirth, request.City, request.Bio, request.Country,request.PostalCode, request.Address, request.Gender, request.PhoneNo, request.Nick);
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
        public async Task<ActionResult<UpdateProfileAvatarResponse>> UpdateProfileAvatar([FromBody] UpdateProfileAvatarRequest request)
        {
            try
            {
                UpdateProfileAvatarResponse response = new UpdateProfileAvatarResponse { FileName = "" };
                if (request.RemoveAvatar)
                {
                    await _dataImpl.RemoveAvatar(request.AuthToken);
                    return Ok(response);
                }
                else
                {
                    string file = await _dataImpl.UpdateProfileAvatar(request.AuthToken, request.PhotoBase64);
                    response.FileName = file;
                    return Ok(response);
                }
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


        [Authorize]
        [HttpGet("/api/[controller]/stripe/connectedaccount")]
        public async Task<IActionResult> GetStripeConnectedAccountLink()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var role = User.GetUserRole();

                if (!(role == Model.UserRoleType.COACH.ToString()))
                {
                    throw new CoachOnlineException("Wrong account type.", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _userSvc.UpdateUserStripeConnectedAccountLink(userId.Value);
                return Ok(data);
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserProfileData(string authToken)
        {
            try
            {
                var user = await _userSvc.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                var data = await _userSvc.GetUserProfileData(user);
                return Ok(data);
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

        [Authorize]
        [HttpGet("/api/[controller]/paymethods")]
        public async Task<IActionResult> GetUserPaymentMethods()
        {
            try
            {
                var userId = User.GetUserId();

                if(!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }

                var data = await _subscriptionSvc.GetUserPaymentMethods(userId.Value);

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


        [Authorize]
        [HttpDelete("/api/[controller]/paymethods/{payMethod}")]
        public async Task<IActionResult> DeleteUserPaymentMethod(string payMethod)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }

                await _subscriptionSvc.DeleteUserPaymentMethod(userId.Value, payMethod);

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

        [Authorize]
        [HttpPatch("/api/[controller]/paymethods/{payMethod}")]
        public async Task<IActionResult> UpdateUserPaymentMethod(string payMethod,[FromBody] PaymentMethodBillingDetailsOptions opts)
        {
            try
            {
                var userId = User.GetUserId();

                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authorized", CoachOnlineExceptionState.NotAuthorized);
                }

                await _subscriptionSvc.UpdateUserPaymentMethod(userId.Value, payMethod, opts);

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                Console.WriteLine(e.ToString());
                _logger.LogError(e.Message);

                return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}
