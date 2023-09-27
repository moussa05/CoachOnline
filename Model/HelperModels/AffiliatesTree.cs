using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.HelperModels
{
    public class AffiliatesTree
    {
        public int HostUserId { get; set; }
        public List<AffiliateChild> AffiliateChildren { get; set; }
    }

    public class AffiliateChild
    {
        public int DirectHostId { get; set; }
        public int AffiliateUserId { get; set; }
        public bool IsFirstGeneration { get; set; }
        public DateTime JoinDate { get; set; }
        public bool IsCoach { get; set; }
        public AffiliateModelType AffiliateModelType { get; set; }
    }
}
