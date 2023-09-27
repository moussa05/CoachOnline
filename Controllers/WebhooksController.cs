using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Statics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Rollbar;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {

        
        public WebhooksController(ILogger<WebhooksController> _logger, IWebhook _webhookParser)
        {
            this.logger = _logger;
            this.webhookParser = _webhookParser;
        }
        IWebhook webhookParser;
        ILogger<WebhooksController> logger;

        [HttpPost]
        public async Task<ActionResult> ConnectedWebhook()
        {
            try
            {
                if (Request.Headers != null && Request.Headers.ContainsKey("Stripe-Signature"))
                {
                    string bodyStr = "";
                    using (var rd = new System.IO.StreamReader(Request.Body))
                    {
                        bodyStr = await rd.ReadToEndAsync();
                    }

                    var stripeEvent = EventUtility.ConstructEvent(bodyStr, Request.Headers["Stripe-Signature"], ConfigData.Config.StripeWebhookKey);


                    var dataObj = stripeEvent.Data.Object;
                    var subscription = dataObj as Subscription;
                    switch (stripeEvent.Type)
                    {
                        case Events.CustomerSubscriptionUpdated:
                            if (subscription != null)
                            {
                                await webhookParser.SubscriptionUpdated(subscription);
                            }
                            break;
                        case Events.CustomerSubscriptionCreated:
                            if (subscription != null)
                            {
                                await webhookParser.SubscriptionCreated(subscription);
                            }
                            break;
                        case Events.CustomerSubscriptionDeleted:
                            if (subscription != null)
                            {
                                await webhookParser.SubscriptionCancelled(subscription);
                            }
                            break;
                        case Events.SubscriptionScheduleReleased:
                            var subscriptionSchedule = dataObj as SubscriptionSchedule;
                            if (subscriptionSchedule != null)
                            {
                                await webhookParser.ScheduleReleased(subscriptionSchedule);
                            }
                            break;
                        case Events.PaymentIntentRequiresAction:
                            var payIntent = dataObj as PaymentIntent;
                            if (payIntent != null)
                            {
                                if (payIntent.Status == "requires_action")
                                {
                                    await webhookParser.PayIntentRequiresAction(payIntent);
                                }
                            }
                            break;
                        //case Events.PaymentIntentPaymentFailed:
                        //    var payIntent2 = dataObj as PaymentIntent;
                        //    if (payIntent2 != null)
                        //    {
                        //        if (payIntent2.Status == "requires_action" || payIntent2.Status == "requires_payment_method")
                        //        {
                        //            await webhookParser.PayIntentRequiresAction(payIntent2);
                        //        }
                        //    }
                        //    break;
                        //case Events.AccountUpdated:
                        //    var account = dataObj as Account;
                        //    if (account != null)
                        //    {
                        //        await webhookParser.ChangeUserState(account);
                        //    }
                        //    break;
                        default:break;

                    }

                }

                return Ok();
            }
            catch (CoachOnlineException e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogInformation(e.Message);
                return new CoachOnlineActionResult(e);
            }
            catch (Exception e)
            {
                //RollbarLocator.RollbarInstance.AsBlockingLogger(TimeSpan.FromSeconds(1)).Error(e);

                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException(e.ToString(), CoachOnlineExceptionState.UNKNOWN));
            }
        }



        [HttpPost]
        public async Task<ActionResult> ConnectedWebhookAccount()
        {
            try
            {
                if (Request.Headers != null && Request.Headers.ContainsKey("Stripe-Signature"))
                {
                    string bodyStr = "";
                    using (var rd = new System.IO.StreamReader(Request.Body))
                    {
                        bodyStr = await rd.ReadToEndAsync();
                    }
                    var stripeEvent = EventUtility.ConstructEvent(bodyStr, Request.Headers["Stripe-Signature"], ConfigData.Config.StripeWebhookKeyAccount);


                    var dataObj = stripeEvent.Data.Object;
                    switch (stripeEvent.Type)
                    {
                        case Events.AccountUpdated:
                            var account = dataObj as Account;

                            if (account != null)
                            {
                                Console.WriteLine("Account has been updated");
                                await webhookParser.ChangeUserState(account);
                            }
                            break;
                        default:
                            break;

                    }

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
                logger.LogError(e.Message);
                return new CoachOnlineActionResult(new CoachOnlineException(e.ToString(), CoachOnlineExceptionState.UNKNOWN));
            }
        }
    }
}
