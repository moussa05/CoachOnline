using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class AdminAffiliateInfoResp
    {
        public int UserId { get; set; }
        public string AffiliateLink { get; set; }
        public string AffiliateLinkForCoaches { get; set; }
        public List<AffiliateHostPaymentsAPI> TotalEarnings { get; set; }
        public List<AffiliateAPI> Affiliates { get; set; }
    }
}
