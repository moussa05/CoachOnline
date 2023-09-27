using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class ConfirmNewPasswordRequest
    {
        public string emailAddress { get; set; }
        public string Password { get; set; }
        public string RepeatedPassword { get; set; }
        public string ResetPasswordToken { get; set; }
    }
}
