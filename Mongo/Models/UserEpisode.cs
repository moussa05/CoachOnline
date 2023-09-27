using CoachOnline.Mongo.Generics;
using CoachOnline.Mongo.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo.Models
{
    [BsonCollection("UserEpisodes")]
    public class UserEpisode: IMongoCollection
    {
        public Guid Id { get; set; }
        [BsonElement("episodeId")]
        public int EpisodeId { get; set; }
        [BsonElement("userId")]
        public int UserId { get; set; }
        [BsonElement("movieLength")]
        public decimal Duration { get; set; }
        [BsonElement("timestamps")]
        public List<EpisodeTimestamp> Timestamps { get; set; } = new List<EpisodeTimestamp>();
    }

    public class EpisodeTimestamp
    {
        [BsonElement("value")]
        public decimal Value { get; set; }
        [BsonElement("rate")]
        public decimal Rate { get; set; }
        [BsonElement("updateTime")]
        public DateTime UpdateTime { get; set; }
    }
}
