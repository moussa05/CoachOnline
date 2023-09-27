using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CreateCategoryRequest
    {
        public string AuthToken { get; set; }
        public string Name { get; set; }
    }
}
