using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class ReSuggestCoursesRqs
    {
        [Required]
        public DateTime ForDay { get; set; }
        [Required]
        public int CountFromMonths { get; set; }
    }
}
