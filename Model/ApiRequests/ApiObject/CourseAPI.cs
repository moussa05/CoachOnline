using CoachOnline.Model.ApiResponses.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.ApiObject
{
    public class CourseAPI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CourseState State { get; set; }
        public CategoryAPI Category { get; set; }
        public string Description { get; set; }
        public virtual List<EpisodeAPI> Episodes { get; set; }
        public string PhotoUrl { get; set; }
        public long Created { get; set; }
        public bool HasPromo { get; set; }
        public string BannerPhotoUrl { get; set; }
    }
}
