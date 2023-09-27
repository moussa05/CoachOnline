using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Model.ApiRequests;
using CoachOnline.PayPalIntegration;
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
    public class PayPalController : ControllerBase
    {

        private readonly ILogger<PayPalController> _logger;
        private readonly IPayPal _paypalSvc;
        private readonly ICoach _coachSvc;
        private readonly IAffiliate _affSvc;
        public PayPalController(ILogger<PayPalController> logger, IPayPal paypalSvc, ICoach coachSvc, IAffiliate affSvc)
        {
            _paypalSvc = paypalSvc;
            _logger = logger;
            _coachSvc = coachSvc;
            _affSvc = affSvc;
        }


        [HttpGet]
        public async Task<IActionResult> GetPayPalAccountInfo()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (!(userRole == UserRoleType.COACH.ToString() || userRole == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("User has incorrect account type", CoachOnlineExceptionState.NotAuthorized);
                }
                var result = await _paypalSvc.GetUserPayPalAccountInfo(userId.Value);
                return new OkObjectResult(result);
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
        public async Task<IActionResult> VerifyAccount([FromBody] PayPalAccessTokenRqs rqs)
        {
            try
            {
                var userId = User.GetUserId();
                if(!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if(!(userRole == UserRoleType.COACH.ToString() || userRole == UserRoleType.STUDENT.ToString()))
                {
                    throw new CoachOnlineException("User cannot perform this action due to his account limitations.", CoachOnlineExceptionState.NotAuthorized);
                }
                await _paypalSvc.VerifyAccount(userId.Value, rqs.PayPalAccessToken);
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


        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> WithdrawTest(int id, string payerId, bool isEmail)
        {
            try
            {
                await _paypalSvc.Payout(payerId, 2, "EUR", "Test", "Test", "To jest test", "xyz"+id.ToString(), isEmail);
                
                return Ok();

            }
            catch (CoachOnlineException e)
            {

                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                return new CoachOnlineActionResult(new CoachOnlineException(e.ToString(), CoachOnlineExceptionState.UNKNOWN));
            }
        }

        [HttpPost]
        public async Task<IActionResult> WithdrawPaymentByPaypal()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var userRole = User.GetUserRole();
                if (userRole != UserRoleType.COACH.ToString())
                {
                    throw new CoachOnlineException("User is not a coach", CoachOnlineExceptionState.NotAuthorized);
                }
                await _paypalSvc.WithdrawPaymentByPaypal(userId.Value);
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


        [HttpPost("/api/[controller]/widthraw/all")]
        public async Task<IActionResult> WithdrawAllPaymentsByPaypal()
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User not authenticated", CoachOnlineExceptionState.NotAuthorized);
                }
                bool anythingWidthrawn = false;
                var userRole = User.GetUserRole();
                if (userRole == UserRoleType.COACH.ToString())
                {
                    var coach = await _coachSvc.GetCoachByIdAsync(userId.Value);
                    var data = await _coachSvc.GetCurrentAmountToWidthraw(coach);
                    if (data.ToWidthraw > 0)
                    {
                        await _paypalSvc.WithdrawPaymentByPaypal(userId.Value);
                        anythingWidthrawn = true;
                    }
                }

                var affMoney = await _affSvc.GetEarnedMoneyfromAffiliatesGeneral(userId.Value);
                if (affMoney != null && affMoney.Any(x => x.ToWithdraw > 0))
                {
                    await _affSvc.WithdrawPaymentByPaypal(userId.Value);
                    anythingWidthrawn = true;
                }

                if(!anythingWidthrawn)
                {
                    throw new CoachOnlineException("No funds to perform payout", CoachOnlineExceptionState.CantChange);
                }
               
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
    }
}
