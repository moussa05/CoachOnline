using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiRequests.Admin
{
    
    public class GetCategoriesResponse
    {
        public List<GetAdminCategoriesItem> items { get; set; }

    }

    public class GetAdminCategoriesResponse
    {
        public List<GetAdminCategoriesItem> items { get; set; }
    }

    public class GetAdminCategoriesRequest
    {
        [Required]
        public string AdminAuthToken { get; set; }
    }


    public class GetAdminCategoriesItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GetAdminCategoriesFamily> Children { get; set; }
    }
    public class GetAdminCategoriesFamily
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
