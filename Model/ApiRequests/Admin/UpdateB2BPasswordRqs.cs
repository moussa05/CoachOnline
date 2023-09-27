using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateB2BPasswordAdminRqs
    {
        [Required]
        public string Password { get; set; }
        [Required]
        public string RepeatPassword { get; set; }
    }

    public class UpdateB2BPasswordUserRqs
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RepeatPassword { get; set; }
    }
}
