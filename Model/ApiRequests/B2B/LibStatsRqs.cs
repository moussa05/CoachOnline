using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{

    public class LibStatsRqs
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string Key { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }


    }

    public class LibStatsRqsWithoutToken
    {
        [Required]
        public string Key { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }


    }
}
