using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class LinkOptsResponse
    {
        public string Link { get; set; }
        public bool LimitedPageView { get; set; }
        public string CouponId { get; set; }
        public CouponResponse Coupon { get; set; }
        public string ReturnUrl { get; set; }
        public bool WithTrialPlans { get; set; }
    }
}
