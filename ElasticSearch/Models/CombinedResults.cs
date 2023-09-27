using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Models
{
    public class CombinedResults
    {
        public IReadOnlyCollection<CourseIndex> Courses { get; set; }
        public IReadOnlyCollection<CoachIndex> Coaches { get; set; }
        public IReadOnlyCollection<CategoryIndex> Categories { get; set; }
    }
}
