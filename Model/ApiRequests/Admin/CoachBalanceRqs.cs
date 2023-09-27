using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class CoachBalanceRqs
    {
        [Required]
        public string AdminAuthToken { get; set; }
        [Required]
        public int CoachId { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
    }


    public class CoachOwnBalanceRqs
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
