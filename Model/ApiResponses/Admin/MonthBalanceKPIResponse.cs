using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class MonthBalanceKPIResponse
    {
        public DateTime Month { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal StripeFees { get; set; }
        public decimal MoneyForCoaches { get; set; }
        public decimal MoneyEarnedByCoaches { get; set; }
        public decimal MoneyForAffiliation { get; set; }
        public string Currency { get; set; }
        public string AffiliationCurrency { get; set; }
    }
}
