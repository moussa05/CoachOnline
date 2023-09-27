using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.Student
{
    public class UpdateEndUserProfileRqs
    {
        public string AuthToken { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int? YearOfBirth { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public string PhoneNo { get; set; }
        public string Nick { get; set; }
    }

    public class UpdateUerEmailRqs
    {
        public string AuthToken { get; set; }
        public string Email { get; set; }
    }

    public class ConfirmEmailChangeRqs
    {
        public string EmailChangeToken { get; set; }
    }
}
