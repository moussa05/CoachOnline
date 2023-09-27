using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model.ApiRequests;
using CoachOnline.Model.Student;
using CoachOnline.Model.SubscriptionModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers.EndUser
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        ILogger<SubscriptionController> _logger;
        ISubscription _sub;
        IUser _user;
        public SubscriptionController(ILogger<SubscriptionController> logger, ISubscription sub, IUser user)
        {
            _logger = logger;
            _sub = sub;
            _user = user;
        }
      
        [HttpGet]
        public async Task<IActionResult> GetAvailableSubscriptionTypes(string authToken, string affToken)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(authToken))
                {
                    if (string.IsNullOrWhiteSpace(affToken))
                    {
                        var data = await _sub.GetSubscriptionPlans();
                        return new OkObjectResult(data);
                    }
                    else
                    {
                        var data = await _sub.GetSubscriptionPlans(null, affToken);
                        return new OkObjectResult(data);
                    }
                }
                else
                {
                    var user = await _user.GetUserByTokenAllowNullAsync(authToken);
                    int? userId = null;
                    userId = user != null ? (int?)user.Id : null;
                    var data = await _sub.GetSubscriptionPlans(userId) ;
                    return new OkObjectResult(data);
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



        [HttpGet]
        public async Task<IActionResult> GetUserCurrentSubscriptionPlan(string authToken)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                var data = await _sub.GetUserCurrentSubscriptionPlan(user.Id);
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

        [HttpGet]
        public async Task<IActionResult> GetUserActiveSubscriptionPlan(string authToken)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                var data = await _sub.GetUserActiveSubscriptionPlan(user.Id);
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

        [HttpGet]
        public async Task<IActionResult> GetAllUserSubscriptions(string authToken)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                var data = await _sub.GetAllUserSubscriptionPlans(user.Id);
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

        [HttpPost]
        public async Task<IActionResult> CreateStudentStripeAccount([FromBody] AuthTokenOnlyRequest token)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(token.AuthToken);
                user.CheckExist("User");
                await _sub.CreateUserStripeCustomerAccount(user);
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
        public async Task<IActionResult> CreateUserSubscription([FromBody]AuthTokenOnlyRequest token)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(token.AuthToken);
                user.CheckExist("User");
                await _sub.AddUserSubscription(user);
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
        public async Task<IActionResult> CancelSubscription(SubscriptionPlanCancelRqs rqs)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");
                var response = await _sub.CancelSubscription(user, rqs.UserCancelSubResponse);
                return new OkObjectResult(response);
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
        public async Task<IActionResult> ChangeSubscriptionPlan(SubscriptionPlanChangeRqs rqs)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");
                var response = await _sub.ChangeSubscription(user, rqs.NewSubscriptionPlanId);
                return new OkObjectResult(response);
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

        //[HttpPost]
        //public async Task<IActionResult> DeleteSubscriptionPlan([FromBody]SubscriptionPlanDeleteRqs rqs)
        //{
        //    try
        //    {
        //        var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
        //        user.CheckExist("User");
        //        await _sub.DeleteSubscriptionPlan(user.Id, rqs.SubscriptionPlanId);
        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> CancelScheduledSubscription([FromBody] SubscriptionPlanDeleteRqs rqs)
        //{
        //    try
        //    {
        //        var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
        //        user.CheckExist("User");
        //        await _sub.CancelScheduledSubscription(user.Id, rqs.SubscriptionPlanId);
        //        return Ok();
        //    }
        //    catch (CoachOnlineException e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(e);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e.Message);
        //        return new CoachOnlineActionResult(new CoachOnlineException("Unknown error. Check logs.", CoachOnlineExceptionState.UNKNOWN));
        //    }
        //}

        [Authorize]
        [HttpPost("/api/[controller]/pintent/{userSubId}")]
        public async Task<IActionResult> GetSubPaymentIntent(int userSubId)
        {
            try
            {
                var userId = User.GetUserId();
                if (!userId.HasValue)
                {
                    throw new CoachOnlineException("User Not Authenticated", CoachOnlineExceptionState.NotAuthorized);
                }

                var result = await _sub.GetPaymentIntentForSub(userId.Value, userSubId);
                return new OkObjectResult(result);
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
        public async Task<IActionResult> CreateSetupIntent([FromBody] AuthTokenOnlyRequest rqs)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");
                var result = await _sub.CreateSetupIntent(user);
                return new OkObjectResult(result);
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

        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetails(string authToken)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                var data = await _sub.GetSubscriptionInvoices(user);

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

        [HttpGet]
        public async Task<IActionResult> GetUserDefaultCardInfo(string authToken)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(authToken);
                user.CheckExist("User");
                bool result = await _sub.IsCustomerDefaultPaymentACard(user);
                if(!result)
                {
                    return new OkObjectResult(new PaymentMethodResponse());
                }
                var data = await _sub.GetCustomerDefaultPaymentMethod(user);
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


        [HttpPost]
        public async Task<IActionResult> SelectSubscriptionPlan([FromBody]SubscriptionPlanAddRqs rqs)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");
                var data = await _sub.SelectUserSubscriptionPlan(rqs.SubscriptionId, user.Id);
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

        [HttpPost]
        public async Task<IActionResult> SetCustomerDefaultSource([FromBody] CustomerDefaultPayMthodRqs rqs)
        {
            try
            {
                var user = await _user.GetUserByTokenAsync(rqs.AuthToken);
                user.CheckExist("User");
                await _sub.SetCustomerDefaultSource(user, rqs.PayMethodId);
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
        public async Task<IActionResult> UploadStudentCard([FromBody]StudentCardUpload upload)
        {
            try
            {
                Console.WriteLine("Subscription id is: "+upload.SubscriptionPlanId);
                var user = await _user.GetUserByTokenAsync(upload.AuthToken);
                user.CheckExist("User");
                await _sub.UploadStudentCardForSubscription(upload.SubscriptionPlanId, upload.StudentCardBase64Imgs);

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
    }
}
