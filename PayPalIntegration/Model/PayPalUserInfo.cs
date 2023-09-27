using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration.Model
{
    public class PayPalUserInfo
    {
        public string user_id { get; set; }
        public string payer_id { get; set; }
        public string name { get; set; }
        public string family_name { get; set; }
        public string given_name { get; set; }
        public List<PayPalEmails> emails { get; set; }
        public bool verified_account { get; set; }
    }

    public class PayPalEmails
    {
        public string value { get; set; }
        public bool primary { get; set; }
    }
}
