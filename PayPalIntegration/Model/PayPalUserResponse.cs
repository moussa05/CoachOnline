using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration.Model
{
    public class PayPalUserResponse
    {
        public string PayPalEmail { get; set; }
        public string PayPalPayerId { get; set; }
        public int UserId { get; set; }
    }
}
