using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Interfaces
{
    public interface IWebhook
    {
        Task ChangeUserState();
        Task<Model.User> ChangeUserState(Account account);
        Task SubscriptionCancelled(Subscription subscription);
        Task SubscriptionUpdated(Subscription subscription);
        Task SubscriptionCreated(Subscription subscription);
        Task ScheduleReleased(SubscriptionSchedule schedule);
        Task PayIntentRequiresAction(PaymentIntent pi);
    }
}
