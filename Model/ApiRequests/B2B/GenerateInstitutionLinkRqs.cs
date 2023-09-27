using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.B2B
{
    public class GenerateInstitutionLinkRqs
    {
        public string ProposedName { get; set; }
    }

    public class GenerateInstitutionLinkRqsWithToken: GenerateInstitutionLinkRqs
    {
        public string Token { get; set; }
    }
}
