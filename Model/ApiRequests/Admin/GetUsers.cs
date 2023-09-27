using CoachOnline.Model.ApiRequests.ApiObject;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    public class GetUsersRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
        public int Count { get; set; }
        public int Last { get; set; }
        public bool FromOldest { get; set; }
        public bool FilterByRole { get; set; } = false;
        public string Role { get; set; } = "STUDENT";
    }
    public class GetUsersResponse
    {

        public List<UserAPI> Users { get; set; }
        public int TotalCount { get;set; }
    }
}

