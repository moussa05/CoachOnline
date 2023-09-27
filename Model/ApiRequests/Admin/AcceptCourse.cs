using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class AcceptCourseRequest
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
    }

}
