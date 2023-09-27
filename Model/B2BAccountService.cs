using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model
{
    public class B2BAccountService
    {
        public int B2BAccountId { get; set; }
        public virtual B2BAccount B2BAccount { get; set; }
        public int ServiceId { get; set; }
        public virtual B2BPricing Service { get; set; }
        public decimal? Comission { get; set; }
        public string ComissionCurrency { get; set; }
    }
}
