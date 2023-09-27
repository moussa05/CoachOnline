using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class PromoCoupon
    {
        [Key]
        public string Id { get; set; }
        public string Name { get; set; }
        public CouponDuration Duration {get;set;}
        public int? PercentOff { get; set; }
        public int? DurationInMonths { get; set; }
        public decimal? AmountOff { get; set; }
        public string Currency { get; set; }
        public bool AvailableForInfluencers { get; set; }
    }

    public enum CouponDuration : int
    {
        once,
        forever,
        repeating
    }
}
