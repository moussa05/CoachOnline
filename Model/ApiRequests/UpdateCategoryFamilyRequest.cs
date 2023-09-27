using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class UpdateCategoryFamilyRequest
    {
        public string AdminAuthToken { get; set; }
        public int CategoryId { get; set; }
        public List<int> Children { get; set; }
    }
}
