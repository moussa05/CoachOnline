using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class AffiliateHostPaymentsAPI
    {
        public int UserId { get; set; }
        public decimal Total { get; set; }
        public decimal Withdrawn { get; set; }
        public decimal ToWithdraw { get; set; }
        public string Currency { get; set; }
    }
}
