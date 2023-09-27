using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class GetAuthTokenRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string DeviceInfo { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public string PlaceInfo { get; set; } = "";
    }
    public class GetAuthTokenResponse
    {
        public string AuthToken { get; set; }
        public UserAuthInfo UserInfo { get; set; }
    }

    public class UserAuthInfo
    {
        public string Name { get; set; }
        public string Email { get; set; }
        [Range(0,3)]
        public int StripeVerificationStatus { get; set; }
        public bool SubscriptionActive { get; set; }
        public string UserRole { get; set; }
        public string StripeCustomerId { get; set; }
        //public bool HasStripeAccount { get; set; }
        //public bool FirstStageVerifed { get; set; }
        //public bool SecondStageVerifed { get; set; }
        /*
         
0 - Nothing
1 - Only stripe account
2 - Only Deposits
3 - Everything 
         */
    }
}
