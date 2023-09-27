using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class MonthlyBalance
    {
        [Key]
        public int Id { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public long BalanceFull { get; set; }
        public decimal Fees { get; set; }
        public decimal BalancaeForWithdrawals { get; set; }
        public string Currency { get; set; }
        public DateTime CalculationDate { get; set; }
    }
}
