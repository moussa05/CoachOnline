using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class CategoryIndex
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Name_ { get; set; }
        public int? ParentId { get; set; }
        public string ParentName { get; set; }
        public string ParentName_ { get; set; }
        public bool AdultOnly { get; set; }
        public string AdultContent
        {
            get
            {
                if (AdultOnly)
                    return "Adult +18";
                else return "";
            }
        }
        public List<CourseIndex> Courses { get; set; } = new List<CourseIndex>();
        public List<CategoryIndex> ChildCategories { get; set; } = new List<CategoryIndex>();
        
    }
}
