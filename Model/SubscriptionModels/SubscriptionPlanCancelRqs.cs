using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.SubscriptionModels
{
    public class SubscriptionPlanCancelRqs
    {
        [Required]
        public string AuthToken { get; set; }
        //[Required]
        public int? UserCancelSubResponse { get; set; }
    }

    public class SubscriptionPlanDeleteRqs
    {
        public string AuthToken { get; set; }
        public int SubscriptionPlanId { get; set; }
    }
}
