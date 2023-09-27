using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class AddLibraryReferentRqs
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNo { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string PhotoBase64 { get; set; }
    }

    public class AddLibraryReferentRqsWithToken: AddLibraryReferentRqs
    {
        public string Token { get; set; }
    }
}
