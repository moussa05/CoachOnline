using CoachOnline.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CoachOnline.PayPalIntegration
{
    public class PayPalConfiguration
    {
        public readonly static string ClientId;
        public readonly static string ClientSecret;
        public static HttpClient client = new HttpClient();

        static PayPalConfiguration()
        {
            ClientId = ConfigData.Config.PayPalClientID;
            ClientSecret = ConfigData.Config.PayPalSecret;
        }


      
    }
}
