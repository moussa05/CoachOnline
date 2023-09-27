using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class AddLibrarySubscriptionRqs
    {
        [Required]
        public DateTime SubscriptionStartDate { get; set; }
        [Required]
        public int PricingPlanId { get; set; }

        public decimal? NegotiatedPrice { get; set; }
        public bool AutoRenew { get; set; }
    }

    public class AddLibrarySubscriptionRqsWithToken: AddLibrarySubscriptionRqs
    {
        public string Token { get; set; }
    }
}
