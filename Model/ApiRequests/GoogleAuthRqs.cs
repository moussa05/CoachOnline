using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class GoogleAuthRqs
    {
        [Required]
        public string IdToken { get; set; }
        public string DeviceInfo { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string PlaceInfo { get; set; } = "";
    }

    public class GoogleRegisterRqs
    {
        [Required]
        public string IdToken { get; set; }
        [Required]
        public UserRoleType UserRole { get; set; }

        public int? LibraryId { get; set; }
        public string Gender { get; set; }
        public int? ProfessionId { get; set; }
        public int? YearOfBirth { get; set; }

        public string AffiliateLink { get; set; }


        public string DeviceInfo { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string PlaceInfo { get; set; } = "";
    }
}
