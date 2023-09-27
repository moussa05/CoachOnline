using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
  
    public class AuthTokenOnlyRequest
    {
        public string AuthToken { get; set; }
    }

    public class TokenOnlyRequest
    {
        public string Token { get; set; }
    }
}
