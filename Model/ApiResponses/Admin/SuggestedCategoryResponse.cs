using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Model.ApiResponses.Admin
{
    public class SuggestedCategoryResponse: CategoryAPI
    {
        public string CoachName { get; set; }
        public string CoachEmail { get; set; }
        public int CoachId { get; set; }
     
    }

    public class CategoryAPI
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public int? ParentId { get; set; }
        public bool AdultOnly { get; set; }
        public List<CategoryAPI> ParentsChildren { get; set; }
    }
}
