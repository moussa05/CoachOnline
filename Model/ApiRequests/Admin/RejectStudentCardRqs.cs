using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class RejectStudentCardRqs
    {
        [Required]
        public string AdminAuthToken { get; set; }
        [Required]
        public int SubscriptionId { get; set; }
        [Required]
        public string Reason { get; set; }
    }
}
