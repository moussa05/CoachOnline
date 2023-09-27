using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class PlatformBalanceResponse
    {

        public DateTime Month { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal BalanceForCoaches { get; set; }
        public string Currency { get; set; }
        public decimal TotalWatchedMinutes { get; set; }
    }
}
