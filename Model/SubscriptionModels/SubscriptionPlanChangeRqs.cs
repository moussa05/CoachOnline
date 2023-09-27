using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.SubscriptionModels
{
    public class SubscriptionPlanChangeRqs
    {

        public string AuthToken { get; set; }
        public int NewSubscriptionPlanId { get; set; }
    }


}
