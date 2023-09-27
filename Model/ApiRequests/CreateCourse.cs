using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CreateCourseRequest
    {
        public string AuthToken { get; set; }
        public string Name { get; set; }
        public int? Category { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class CreateCourseResponse
    {
        public int CourseId { get; set; }
    }
}
