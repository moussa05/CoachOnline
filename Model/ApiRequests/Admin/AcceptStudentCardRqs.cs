using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class AcceptStudentCardRqs
    {
        [Required]
        public string AdminAuthToken { get; set; }
        [Required]
        public int SubscriptionId { get; set; }
    }
}
