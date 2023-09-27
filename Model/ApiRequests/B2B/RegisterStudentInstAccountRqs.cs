using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class RegisterStudentInstAccountRqs
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string RepeatPassword { get; set; }
        [Required]
        public int LibraryId { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public int ProfessionId { get; set; }
        [Required]
        public int YearOfBirth { get; set; }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Region { get; set; }
    }
}
