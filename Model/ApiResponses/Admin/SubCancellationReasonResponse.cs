using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class SubCancellationReasonResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int SubscriptionId { get; set; }
        public string CurrentStatus { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string PlanName { get; set; }
        public string Reason { get; set; }
    }
}
