using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class CouponResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CouponDuration Duration { get; set; }
        public string DurationStr
        {
            get
            {
                return Duration.ToString();
            }
        }
        public decimal? PercentOff { get; set; }
        public decimal? AmountOff { get; set; }
        public string Currency { get; set; }
        public int? DurationInMonths { get; set; }
        public bool AvailableForInfluencers { get; set; }
    }
}
