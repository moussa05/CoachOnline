using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class SuggestCategoryRqs
    {
        public string CategoryName { get; set; }
        public int? ParentId { get; set; }
        public bool HasParent { get; set; }
        public bool AdultOnly { get; set; }
    }
}
