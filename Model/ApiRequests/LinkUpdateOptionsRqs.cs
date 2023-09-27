using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class LinkUpdateOptionsRqs
    {
        public bool LimitedPageView { get; set; }
        public string CouponId { get; set; }
        public string ReturnUrl { get; set; }
        public bool WithTrialPlans { get; set; }
    }
}
