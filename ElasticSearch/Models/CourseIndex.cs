using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Models
{
    [ElasticsearchType(IdProperty = nameof(CourseId))]
    public class CourseIndex
    {
        public int CourseId { get; set; }
        public int CategoryId { get; set; }
        public int CoachId { get; set; }
        public string CourseName { get; set; }
        public string CourseName_ { get; set; }
        public string CourseDescription { get; set; }
        public string CourseDescription_ { get; set; }
        public string CoursePhotoUrl { get; set; }
        public string BannerPhotoUrl { get; set; }
        public bool HasPromo { get; set; }
        public CoachIndex Coach { get; set; }
        public CategoryIndex Category { get; set; }
        public List<EpisodeIndex> Episodes { get; set; }
        public int LikesCnt { get; set; }
    }
}
