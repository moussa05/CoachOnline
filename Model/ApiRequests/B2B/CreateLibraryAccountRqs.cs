using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class CreateLibraryAccountRqs
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RepeatPassword { get; set; }

    }

    public class CreateLibraryAccountWithTokenRqs:CreateLibraryAccountRqs
    {
        [Required]
        public string Token { get; set; }
    }
}
