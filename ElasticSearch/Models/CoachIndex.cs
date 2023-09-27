using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Models
{
    [ElasticsearchType(IdProperty = nameof(UserId))]
    public class CoachIndex
    {
        
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstName_ { get; set; }
        public string LastName_ { get; set; }
        public string Bio { get; set; }
        public string Bio_ { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string PhotoUrl { get; set; }
        public string Gender { get; set; }
        public string ProfilePhotoUrl { get
            {
                return $"images/{PhotoUrl}";
            } }
        public List<CoachCategories> Categories { get; set; }
        public List<CourseIndex> Courses { get; set; }
    }

    public class CoachCategories
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
