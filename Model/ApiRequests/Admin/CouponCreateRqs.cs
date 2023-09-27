using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using static CoachOnline.Services.ProductManageService;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class CouponCreateRqs
    {
        [Required]
        public string Name { get; set; }
        public CouponDuration Duration { get; set; }
        public int? PercentOff { get; set; }
        public decimal? AmountOff { get; set; }
        public string Currency { get; set; }
        public int? DurationInMonths { get; set; }
        public bool ForInfluencers { get; set; }
    }

    public class CouponUpdateRqs
    {
        [Required]
        public string Name { get; set; }
        public bool ForInfluencers { get; set; }
    }
}
