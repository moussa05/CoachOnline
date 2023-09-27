using CoachOnline.Mongo.Generics;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoachOnline.Mongo.Interfaces;

namespace CoachOnline.Mongo.Models
{
    [BsonCollection("InstituteUserConnections")]
    public class InstituteUserConnection : IMongoCollection
    {
        public Guid Id { get; set; }
        [BsonElement("userId")]
        public int UserId { get; set; }
        [BsonElement("instituteId")]
        public int InstituteId { get; set; }
        [BsonElement("connectionStartTime")]
        public DateTime ConnectionStartTime { get; set; }
        [BsonElement("connectionEndTime")]
        public DateTime? ConnectionEndTime { get; set; }
        [BsonElement("connectionId")]
        public string ConnectionId { get; set; }
        [BsonElement("allowedToView")]
        public bool? IsAllowedToView { get; set; }
    }
}
