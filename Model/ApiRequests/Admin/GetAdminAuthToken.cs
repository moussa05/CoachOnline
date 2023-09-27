using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class GetAdminAuthTokenResponse
    {
        public string AdminAuthToken { get; internal set; }
    }

    public class GetAdminAuthTokenRequest
    {
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
