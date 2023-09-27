using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class CoachBalanceMonth
    {
        [Key]
        public int Id { get; set; }
        public int CoachId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public virtual User Coach { get; set; }
        public List<CoachBalanceDay> DayBalances { get; set; }
        public int MonthlyBalanceId { get; set; }
        public virtual MonthlyBalance MonthlyBalance { get; set; }
        public decimal TotalMonthBalance { get; set; }
    }

    public class CoachBalanceDay
    {
        [Key]
        public int Id { get; set; }
        public int CoachBalanceMonthId { get; set; }
        public virtual CoachBalanceMonth CoachBalanceMonth { get; set; }
        public DateTime BalanceDay { get; set; }
        public decimal BalanceValue { get; set; }
        public bool Transferred { get; set; }
        public bool Calculated { get; set; }
        public bool? PayoutViaPaypal { get; set; } = null;
        public string PayPalPayoutId { get; set; }
        public decimal TotalEpisodesWatchTime { get; set; }
        public DateTime? TransferDate { get; set; }
        public int? RequestedPaymentId { get; set; }
        public RequestedPayment RequestedPayment { get; set; }
    }
}
