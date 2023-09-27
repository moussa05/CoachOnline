using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.ElasticSearch.Models
{
    [ElasticsearchType(IdProperty = nameof(EpisodeId))]
    public class EpisodeIndex
    {
        public int EpisodeId { get; set; }
        public int OrdinalNumber { get; set; }
        public string Title { get; set; }
        public string Title_ { get; set; }
        public string Description { get; set; }
        public int CourseId { get; set; }
        public bool IsPromo { get; set; }
    }
}
