using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class B2BPricing
    {
        [Key]
        public int Id { get; set; }

        public string PricingName { get; set; }
        public int NumberOfActiveUsers { get; set; }
        public B2BPricingPeriod TimePeriod { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public B2BPricingAccessType AccessType { get; set; }

        [NotMapped]
        public string TimePeriodStr
        {
            get { return TimePeriod.ToString(); }
        }

        [NotMapped]
        public string AccessTypeStr
        {
            get { return AccessType.ToString(); }
        }
    }

    public enum B2BPricingAccessType:byte
    {
        FULL,
        COURSES,
        EVENTS
    }

    public enum B2BPricingPeriod:byte
    {
        WEEKLY,
        MONTHLY,
        QUARTERLY,
        YEARLY
    }
}
