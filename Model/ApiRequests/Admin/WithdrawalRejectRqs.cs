using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class WithdrawalRejectRqs
    {
        [Required]
        public string RejectReason { get; set; }
    }
}
