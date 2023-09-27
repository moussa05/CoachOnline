using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests
{
    public class CategoryToUserRequest
    {
        public string authToken { get; set; }
        public int CategoryId { get; set; }
    }

}
