using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class RemoveEpisodeFromCourseRequest
    {
        public string AuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
    }
}
