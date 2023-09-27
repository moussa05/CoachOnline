using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Student
{
    public class ChangeSubscriptionResponse
    {
        public DateTime ValidFrom { get; set; }
        public int UserSubscriptionId { get; set; }
    }
}
