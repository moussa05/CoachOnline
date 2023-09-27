using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class AffiliateLink
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public string GeneratedLink { get; set; }
        public string GeneratedToken { get; set; }
        public bool ForCoach { get; set; }
        public DateTime CreateDate { get; set; }
        public string CouponCode { get; set; }
        public bool LimitedPageView { get; set; }
        public string ReturnUrl { get; set; }
        public bool WithTrialPlans { get; set; }
    }
}
