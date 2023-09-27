using CoachOnline.Mongo.Generics;
using CoachOnline.Mongo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo
{
    public class MongoCtx
    {
        private readonly IMongoClient _client;
        public MongoCtx(IMongoClient client)
        {
            _client = client;
            UserEpisodes = new UserEpisodeCollection(client);
            InstitureUsersCollection = new InstituteUserConnectionCollection(client);
          //  UserEpisodeIntervals = new UserEpisodeIntervalCollection(client);

        }

        public UserEpisodeCollection UserEpisodes { get; set; }
        public InstituteUserConnectionCollection InstitureUsersCollection { get; set; }
       // public UserEpisodeIntervalCollection UserEpisodeIntervals {get;set;}
    }
}
