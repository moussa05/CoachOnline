using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class GetPossibleCourseStatesResponse
    {
       public List<GetPossibleCourseStatesDate> States { get; set; }
    }
    public class GetPossibleCourseStatesDate
    {
        public CourseState State { get; set; }
        public string StateName { get; set; }
    }
}
