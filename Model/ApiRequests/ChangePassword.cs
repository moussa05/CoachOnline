using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class ChangePasswordRequest
    {
        public string AuthToken { get; set; }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string PasswordRepeat { get; set; }
    }
}
