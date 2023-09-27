using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateAdminEpisodeAttachmentRequest
    {
        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeID { get; set; }
        public string AttachmentHash { get; set; }
    }
}
