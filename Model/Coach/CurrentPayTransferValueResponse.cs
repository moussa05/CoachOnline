using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Coach
{
    public class CurrentPayTransferValueResponse
    {
        public int CoachId { get; set; }
        public decimal CurrentMonthTotalBalance { get; set; }
        public decimal ToWidthraw { get; set; }
        public decimal TotalAmountToWidhdraw
        {
            get { return ToWidthraw; }
        }
        public decimal WithdrawnAmount { get; set; }
        public string Currency { get; set; }
        public List<MonthCoachBalanceResp> Balances { get; set; }
    }

    public class MonthCoachBalanceResp
    {
        public string Currency { get; set; }
        public int BalanceId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal BalanceValue { get; set; }
        public decimal BalanceValueStripe { get; set; }
        public decimal BalanceValuePaypal { get; set; }
        public string Description
        {
            get { return $"Payment for {Month}/{Year} requested on {DateTime.Now}"; }
        }
        public decimal Amonut
        {
            get { return BalanceValue / 100; }
        }

        [JsonIgnore]
        public List<CoachBalanceDay> DailyBalances { get; set; }
    }
}
