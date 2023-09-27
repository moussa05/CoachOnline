using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses
{
    public class PlatformBasicInfoResponse
    {
        public int TotalCoaches { get; set; }
        public int TotalCategories { get; set; }
        public int TotalCourses { get; set; }
        public double TotalMediaTime { get; set; }
    }
}
