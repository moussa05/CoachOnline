using CoachOnline.Mongo.Generics;
using CoachOnline.Mongo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo
{
    public class UserEpisodeCollection: MongoDbRepository<UserEpisode>
    {
        public UserEpisodeCollection(IMongoClient client):base(client)
        {
            
        }

        public IMongoCollection<UserEpisode> Collection
        {
            get { return _collection; }
        }

        public async Task<List<UserEpisode>> FindByUserId(int userId)
        {
            return await _collection.Find(t => t.UserId == userId).ToListAsync();
        }

        public async Task<List<UserEpisode>> FindByEpisodeId(int episodeId)
        {
            return await _collection.Find(t => t.EpisodeId == episodeId).ToListAsync();
        }

        public async Task<List<UserEpisode>> FindByUserIdAndEpisodeId(int episodeId, int userId)
        {
            return await _collection.Find(t => t.UserId==userId && t.EpisodeId == episodeId).ToListAsync();
        }

    }
}
