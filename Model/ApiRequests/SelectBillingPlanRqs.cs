using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class SelectBillingPlanRqs
    {
        public string AuthToken { get; set; }
        public int BillingTypeId { get; set; }
        public string StudentCardBase64 { get; set; }
    }
}
