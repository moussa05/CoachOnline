using ITSAuth.Model;
using Org.BouncyCastle.Asn1.BC;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Interfaces
{
    interface ISMSService
    {
        public void SendMessage(SMS message);
    }
}
