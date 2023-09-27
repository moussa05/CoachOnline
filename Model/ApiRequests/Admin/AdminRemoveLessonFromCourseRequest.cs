using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class AdminRemoveLessonFromCourseRequest
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
    }

    public class AdminRemoveMedia
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
    }
    public class AdminRemoveAttachment
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeId { get; set; }
        public int AttachmentId { get; set; }
    }
}
