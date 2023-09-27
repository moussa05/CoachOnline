using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class RejectSuggestedCategoryRqs
    {
        public int Id { get; set; }
        public string RejectReason { get; set; }
    }
}
