using CoachOnline.Model.ApiResponses.Admin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class SubscriptionPrice
    {
        [Key]
        public int BillingPlanId { get; set; }
        public string StripePriceId { get; set; }
        public string Currency { get; set; }
        public bool Reccuring { get; set; }
        public decimal? Amount { get; set; }
        public int? Period { get; set; }
        public string PeriodType { get; set; }
        public int TrialDays { get; set; }
    }

    public class BillingPlan
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string StripeProductId { get; set; }
        public string Description { get; set; }
        public string StripePriceId { get; set; }
        public string Currency { get; set; }
        public bool IsActive { get; set; }
        public bool IsPublic { get; set; }
        
        public decimal? AmountPerMonth { get; set; }
        public virtual SubscriptionPrice Price { get; set; }
        //public BillingPlanType BillingType { get; set; }
        public BillingPlanOption BillingOption { get; set; }


        [NotMapped]
        public string BillingOptionStr
        {
            get { return BillingOption.ToString(); }
        }

        [NotMapped]
        public bool IsStudentCardRequired { get { return BillingOption == BillingPlanOption.STUDENT; } }

        [NotMapped]
        public decimal? PromotionalPrice { get; set; }

        [NotMapped]
        public decimal? PromotionalAmountPerMonth { get; set; }

        [NotMapped]
        public CouponResponse Coupon { get; set; }

        [JsonIgnore]
        public virtual ICollection<UserBillingPlan> UserBillingPlans { get; set; }
    }

    //public enum BillingPlanType: byte { YEARLY, MONTHLY, WEEKLY, DAILY, QUARTERLY, HALF_YEAR}
    public enum BillingPlanOption: byte { NORMAL, STUDENT}


}
