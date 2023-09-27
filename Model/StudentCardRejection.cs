using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class StudentCardRejection
    {
        [Key]
        public int SubscriptionId { get; set; }
        public string Reason { get; set; }
        public virtual UserBillingPlan Subscription { get; set; }
    }
}
