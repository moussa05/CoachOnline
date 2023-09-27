using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class PaymentMethodsResponse
    {
        public string PaymentMethodId { get; set; }
        public string Last4Digits { get; set; }
        public string Brand { get; set; }
        public int ExpMonth { get; set; }
        public int ExpYear { get; set; }
        public string Country { get; set; }
    }
}
