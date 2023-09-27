using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class UpdateLibraryAccountRqs
    {
        public string LibraryName { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public string StreetNo { get; set; }
        public string PhoneNo { get; set; }
        public string PhotoBase64 { get; set; }
        public string Website { get; set; }

        public int? BooksNo { get; set; }
        public int? ReadersNo { get; set; }
        public int? CdsNo { get; set; }
        public int? VideosNo { get; set; }
        public string SIGBName { get; set; }
    }

    public class UpdateLibraryAccountRqsWithToken: UpdateLibraryAccountRqs
    {
        [Required]
        public string Token { get; set; }
    }
}
