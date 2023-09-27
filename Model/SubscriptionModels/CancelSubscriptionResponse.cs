using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Student
{
    public class CancelSubscriptionResponse
    {
        public DateTime CancellationDate { get; set; }
        public bool IsOkState { get; set; }
        public bool ActiveSubscriptionNotExist { get; set; }
    }
}
