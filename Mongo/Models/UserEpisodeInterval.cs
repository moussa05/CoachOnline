using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo.Models
{
    public class UserEpisodeInterval
    {
        [BsonElement("episodeId")]
        public int EpisodeId { get; set; }
        [BsonElement("userId")]
        public int UserId { get; set; }
        [BsonElement("intervalStart")]
        public decimal IntervalStart { get; set; }
        [BsonElement("intervalEnd")]
        public decimal IntervalEnd { get; set; }
    }
}
