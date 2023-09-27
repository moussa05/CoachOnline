using CoachOnline.Model.ApiRequests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.SubscriptionModels
{
    public class StudentCardUpload: AuthTokenOnlyRequest
    {
        [Required]
        public int SubscriptionPlanId { get; set; }
        [Required]
        public List<PhotoBase64Rqs> StudentCardBase64Imgs { get; set; }
    }
}
