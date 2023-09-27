using CoachOnline.Mongo.Generics;
using CoachOnline.Mongo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo
{
    public class UserEpisodeIntervalCollection : MongoDbRepository<UserEpisodeInterval>
    {
        public UserEpisodeIntervalCollection(IMongoClient client) : base(client)
        {

        }

        public IMongoCollection<UserEpisodeInterval> Collection
        {
            get { return _collection; }
        }

        public async Task<List<UserEpisodeInterval>> FindByUserId(int userId)
        {
            return await _collection.Find(t => t.UserId == userId).ToListAsync();
        }

        public async Task<List<UserEpisodeInterval>> FindByEpisodeId(int episodeId)
        {
            return await _collection.Find(t => t.EpisodeId == episodeId).ToListAsync();
        }

        public async Task<List<UserEpisodeInterval>> FindByUserIdAndEpisodeId(int episodeId, int userId)
        {
            return await _collection.Find(t => t.UserId == userId && t.EpisodeId == episodeId).ToListAsync();
        }
    }
}
