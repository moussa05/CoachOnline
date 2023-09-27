using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CreateAccountRequest
    {
        [Required]
        public string EmailAddress { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RepeatedPassword { get; set; }
        public string AffiliateLink { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }

    }
}
