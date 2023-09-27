using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class UpdateCourseAdminRequest
    {

        public string AdminAuthToken { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string Description { get; set; }
        public string PhotoUrl { get; set; }
        
        public string Prerequisite { get; set; }
        public string Objectives { get; set; }
        public string PublicTargets { get; set; }
        public string CertificationQCM { get; set; }

    }
}
