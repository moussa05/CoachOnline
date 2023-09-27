using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateAdminEpisodeMediaRequest
    {
        public string AuthToken { get; set; }
        public int CourseId { get; set; }
        public int EpisodeID { get; set; }
        //public string MediaHash { get; set; }
    }
}
