using ITSAuth.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITSAuth.Interfaces
{
    interface IClaimer
    {
        public Claims ClaimData(string secretToken, string refreshToken = "");
    }
}
