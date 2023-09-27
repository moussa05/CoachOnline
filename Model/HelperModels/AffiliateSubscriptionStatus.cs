using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.HelperModels
{
    public class AffiliateSubscriptionStatus
    {
        public int UserId { get; set; }
        public bool IsCoach { get; set; }
        public string UserStripeId { get; set; }
        public bool SubscriptionIsActive { get; set; }
        public string StripeSubscriptionId { get; set; }
        public int? SubscriptionId { get; set; }
        public int SubscriptionPeriodMths { get; set; }
        public decimal SubscriptionPrice { get; set; }
        public string Currency { get; set; }
        public string SubscriptionName { get; set; }
        public DateTime? ActiveFromDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? ActualActiveFromDate { get; set; }
        public string SubCancellationReason { get; set; }
    }
}
