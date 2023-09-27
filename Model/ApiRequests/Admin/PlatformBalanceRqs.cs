using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class PlatformBalanceRqs
    {
        [Required]
        public string AdminAuthToken { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
