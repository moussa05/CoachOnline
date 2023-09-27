using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateAdminEpisodeInCourseRequest
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int OrdinalNumber { get; set; }
    }


}
