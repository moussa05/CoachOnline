using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class ConnectedAccountCreateRequest
    {
        [Required]
        public string AuthToken { get; set; }
        public string CountryCode { get; set; }
    }
}
