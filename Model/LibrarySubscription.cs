using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class LibrarySubscription
    {
        [Key]
        public int Id { get; set; }
        public int LibraryId { get; set; }
        public virtual LibraryAccount Library { get; set; }
        public DateTime SubscriptionStart { get; set; }
        public DateTime SubscriptionEnd { get; set; }
        public int PricePlanId { get; set; }
        public virtual B2BPricing PricePlan { get; set; }

        public LibrarySubscriptionStatus Status { get; set; }
        public string PricingName { get; set; }
        public int NumberOfActiveUsers { get; set; }
        public B2BPricingPeriod TimePeriod { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public B2BPricingAccessType AccessType { get; set; }

        public decimal? NegotiatedPrice { get; set; }
        public bool AutoRenew { get; set; }
        public bool? IsProlonged { get; set; }
    }

    public enum LibrarySubscriptionStatus:byte
    {
        AWAITING,
        ACTIVE,
        ENDED,
        CANCELLED
    }
}
