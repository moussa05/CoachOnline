using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class B2BAddPricingRqs
    {
        [Required]
        public string PricingName { get; set; }
        [Required]
        public int NumberOfActiveUsers { get; set; }
        [Required]
        public B2BPricingPeriod TimePeriod { get; set; }
        [Required]
        public B2BPricingAccessType AccessType { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public string Currency { get; set; }
    }

    public class B2BUpdatePricingRqs
    {
   
        public string PricingName { get; set; }

        public int? NumberOfActiveUsers { get; set; }
    
        public B2BPricingPeriod TimePeriod { get; set; }
        public B2BPricingAccessType AccessType { get; set; }

        public decimal? Price { get; set; }
  
        public string Currency { get; set; }
    }
}
