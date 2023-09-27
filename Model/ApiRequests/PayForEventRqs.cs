using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class PayForEventRqs
    {
        public int AssignationId { get; set; }
        public string PaymentMethodId { get; set; }
    }
}
