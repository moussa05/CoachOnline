using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class CourseFlaggedModelRqs
    {
        public string AuthToken { get; set; }
        public List<CourseFlagRqs> FlaggedCourses { get; set; }
    }

    public class CourseFlagRqs
    {
        public int CourseId { get; set; }
        public int OrederNo { get; set; }
    }
}
