using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CustomerDefaultPayMthodRqs
    {
        [Required]
        public string AuthToken { get; set; }

        public string PayMethodId { get; set; }
    }
}
