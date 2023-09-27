using CoachOnline.Model.Student;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class UpdateCoachCVResponse
    {
        public string FileName { get; set; }
    }

    public class UpdateCoachReturnsResponse
    {
        public List<string> Returns { get; set; }
    }

    public class UpdateCoachAttestationsResponse
    {
        public List<string> Diplomas { get; set; }
    }
}
