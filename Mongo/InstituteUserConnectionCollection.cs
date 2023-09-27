using CoachOnline.Mongo.Generics;
using CoachOnline.Mongo.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo
{
    public class InstituteUserConnectionCollection : MongoDbRepository<InstituteUserConnection>
    {
        public InstituteUserConnectionCollection(IMongoClient client) : base(client)
        {

        }

        public IMongoCollection<InstituteUserConnection> Collection
        {
            get { return _collection; }
        }

        public async Task<List<InstituteUserConnection>> FindByUserConnection(string connectionId)
        {
            return await _collection.Find(t => t.ConnectionId== connectionId).ToListAsync();
        }

        public async Task<List<InstituteUserConnection>> FindByUserId(int userId)
        {
            return await _collection.Find(t => t.UserId == userId).ToListAsync();
        }

        public async Task<List<InstituteUserConnection>> FindAllConnected()
        {
            var now = DateTime.Now.AddMinutes(-1);
            return await _collection.Find(t => !t.ConnectionEndTime.HasValue && t.ConnectionStartTime < now).ToListAsync();
        }

        public async Task<List<InstituteUserConnection>> FindByInstituteId(int institureId)
        {
            return await _collection.Find(t => t.InstituteId == institureId).ToListAsync();
        }
    }
}
