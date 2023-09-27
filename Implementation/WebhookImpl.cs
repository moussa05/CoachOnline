using CoachOnline.Helpers;
using CoachOnline.Implementation.Exceptions;
using CoachOnline.Interfaces;
using CoachOnline.Model;
using CoachOnline.Statics;
using ITSAuth.Interfaces;
using ITSAuth.Model;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Implementation
{
    public class WebhookImpl : IWebhook
    {

        private ISubscription _subscriptionSvc;
        private IEmailApiService _emailSvc;
        public WebhookImpl(ISubscription subscriptionSvc, IEmailApiService emailSvc)
        {
            _subscriptionSvc = subscriptionSvc;
            _emailSvc = emailSvc;
        }



        public async Task<Model.User> ChangeUserState(Stripe.Account account)
        {
            using (var cnx = new DataContext())
            {
                var user = cnx.users.Where(x => x.StripeAccountId == account.Id).FirstOrDefault();
                if (user == null)
                {
                    throw new CoachOnlineException("Wrong user id.", CoachOnlineExceptionState.DataNotValid);
                }
                user.PaymentsEnabled = (account.Capabilities.Transfers == "active");
                user.WithdrawalsEnabled = account.PayoutsEnabled;
                await cnx.SaveChangesAsync();

                return user;
            }
        }

        public Task ChangeUserState()
        {
            throw new NotImplementedException();
        }

        public async Task PayIntentRequiresAction(PaymentIntent pi)
        {
            
            var invoice = await _subscriptionSvc.GetInvoiceById(pi.InvoiceId);
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(u => u.StripeCustomerId == pi.CustomerId);
                user.CheckExist("User");
                if (invoice != null && invoice.HostedInvoiceUrl != null)
                {
                    string body = $"<a href='{invoice.HostedInvoiceUrl}'>confirm payment </a> <br><br> Confirmation Url: {invoice.HostedInvoiceUrl}";
                    if (System.IO.File.Exists($"{ConfigData.Config.EnviromentPath}/Emailtemplates/PaymentConfirmation.html"))
                    {
                        body = System.IO.File.ReadAllText($"{ConfigData.Config.EnviromentPath}/Emailtemplates/PaymentConfirmation.html");
                        body = body.Replace("##CONFIRMATIONURL###", $"{invoice.HostedInvoiceUrl}");

                    }
                    await _emailSvc.SendEmailAsync(new EmailMessage
                    {
                        AuthorEmail = "info@coachs-online.com",
                        AuthorName = "Coachs-online",
                        Body = body,
                        ReceiverEmail = user.EmailAddress,
                        ReceiverName = "",
                        Topic = "Coachs-online subscription payment confirmation."
                    });
                }
            }
        }

        public async Task SubscriptionUpdated(Subscription subscription)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(u => u.StripeCustomerId == subscription.CustomerId);
                if (user != null)
                {
                    var subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == user.Id && b.StripeSubscriptionId == subscription.Id);

                    if (subscriptionPlan != null)
                    {
                        if (subscription.Status == "active")
                        {
                            subscriptionPlan.Status = Model.BillingPlanStatus.ACTIVE;
                            subscriptionPlan.ActivationDate = subscription.StartDate;
                            subscriptionPlan.ExpiryDate = subscription.CancelAtPeriodEnd? (DateTime?)subscription.CurrentPeriodEnd: null;
                        }
                        else if (subscription.Status == "trialing")
                        {
                            subscriptionPlan.Status = Model.BillingPlanStatus.ACTIVE;
                            subscriptionPlan.ActivationDate = subscription.StartDate;
                            subscriptionPlan.ExpiryDate = subscription.CancelAtPeriodEnd ? (DateTime?)subscription.CurrentPeriodEnd : null;
                        }
                        else if(subscription.Status == "incomplete")
                        {
                            subscriptionPlan.Status = Model.BillingPlanStatus.AWAITING_PAYMENT;
                        }
                        else
                        {
                            subscriptionPlan.Status = Model.BillingPlanStatus.PENDING;
                        }

                        await ctx.SaveChangesAsync();
                    }

                    await _subscriptionSvc.ChangeUserActiveSubscriptionState(user.Id);
                }
            }
        }

        public async Task SubscriptionCreated(Subscription subscription)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(u => u.StripeCustomerId == subscription.CustomerId);
                if (user != null)
                {
                    
                    var subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == user.Id && b.StripeSubscriptionId == subscription.Id);
                    //possibly subscription schedule
                    if(subscriptionPlan == null)
                    {
                        //get by billing plan id
                        var userBillingPlanId = _subscriptionSvc.GetUserBillingPlanIdFromSubscription(subscription);
                        if(userBillingPlanId.HasValue)
                        {
                            subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b=> b.UserId == user.Id && b.Id == userBillingPlanId);
                        }
                    }

                    if (subscriptionPlan != null)
                    {
                        _subscriptionSvc.SetSubscriptionState(ref subscriptionPlan, subscription);

                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        var priceId = subscription.Items.Data[0].Price.Id;
                        var productId = subscription.Items.Data[0].Price.ProductId;
                        var billingPlanType = await ctx.BillingPlans.Where(t => t.StripePriceId == priceId && t.StripeProductId == productId).Include(p => p.Price).FirstOrDefaultAsync();
                        if (billingPlanType != null)
                        {
                            //create such billing plan for user
                            var subPlan = new UserBillingPlan() { UserId = user.Id, CreationDate = DateTime.Now, BillingPlanTypeId = billingPlanType.Id, Status = BillingPlanStatus.PENDING };
                            if (billingPlanType.BillingOption == BillingPlanOption.STUDENT)
                            {
                                subPlan.IsStudent = true;
                            }
                            else
                            {
                                subPlan.IsStudent = false;
                            }

                            _subscriptionSvc.SetSubscriptionState(ref subscriptionPlan, subscription);

                            ctx.UserBillingPlans.Add(subPlan);
                            await ctx.SaveChangesAsync();
                        }
                    }

                    await _subscriptionSvc.ChangeUserActiveSubscriptionState(user.Id);
                }
            }
        }

        public async Task SubscriptionCancelled(Subscription subscription)
        {
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(u => u.StripeCustomerId == subscription.CustomerId);
                if (user != null)
                {
                    var subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == user.Id && b.StripeSubscriptionId == subscription.Id);

                    if(subscriptionPlan != null)
                    {
                       _subscriptionSvc.SetSubscriptionState(ref subscriptionPlan, subscription);

                       await ctx.SaveChangesAsync();
                    }

                    await _subscriptionSvc.ChangeUserActiveSubscriptionState(user.Id);
                }
            }
        }

        public async Task ScheduleReleased(SubscriptionSchedule schedule)
        {
            if(schedule.SubscriptionId == null)
            {
                return;
            }
            using (var ctx = new DataContext())
            {
                var user = await ctx.users.FirstOrDefaultAsync(u => u.StripeCustomerId == schedule.CustomerId);
                if (user != null)
                {

                    var subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == user.Id && b.StripeSubscriptionId == schedule.SubscriptionId);
                    //possibly subscription schedule
                    if (subscriptionPlan == null)
                    {
                        //get by billing plan id
                        var userBillingPlanId = _subscriptionSvc.GetUserBillingPlanIdFromSubscriptionSchedule(schedule);
                        if (userBillingPlanId.HasValue)
                        {
                            subscriptionPlan = await ctx.UserBillingPlans.FirstOrDefaultAsync(b => b.UserId == user.Id && b.Id == userBillingPlanId);
                        }
                    }

                    if (subscriptionPlan != null)
                    {
                       
                        _subscriptionSvc.SetSubscriptionState(ref subscriptionPlan, schedule.Subscription);

                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        var priceId = schedule.Subscription.Items.Data[0].Price.Id;
                        var productId = schedule.Subscription.Items.Data[0].Price.ProductId;
                        var billingPlanType = await ctx.BillingPlans.Where(t => t.StripePriceId == priceId && t.StripeProductId == productId).Include(p => p.Price).FirstOrDefaultAsync();
                        if (billingPlanType != null)
                        {
                            //create such billing plan for user
                            var subPlan = new UserBillingPlan() { UserId = user.Id, CreationDate = DateTime.Now, BillingPlanTypeId = billingPlanType.Id, Status = BillingPlanStatus.PENDING };
                            if (billingPlanType.BillingOption == BillingPlanOption.STUDENT)
                            {
                                subPlan.IsStudent = true;
                            }
                            else
                            {
                                subPlan.IsStudent = false;
                            }

                            _subscriptionSvc.SetSubscriptionState(ref subscriptionPlan, schedule.Subscription);

                            ctx.UserBillingPlans.Add(subPlan);
                            await ctx.SaveChangesAsync();
                        }
                    }

                    await _subscriptionSvc.ChangeUserActiveSubscriptionState(user.Id);
                }
            }
        }

    }
}
