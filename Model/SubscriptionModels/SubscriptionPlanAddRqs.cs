using CoachOnline.Model.ApiRequests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.SubscriptionModels
{
    public class SubscriptionPlanAddRqs: AuthTokenOnlyRequest
    {
        public int SubscriptionId { get; set; }
    }
}
