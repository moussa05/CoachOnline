using CoachOnline.Mongo.Interfaces;
using CoachOnline.Statics;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo.Generics
{
    public class MongoDbRepository<T> : IMongoDbRepository<T> where T : class
    {
        protected IMongoDatabase Database { get; }
        protected string _collectionName;
        protected IMongoCollection<T> _collection;
        public MongoDbRepository(IMongoClient client)
        {
            Database = client.GetDatabase(ConfigData.Config.MongoDBName);
            _collectionName = GetCollectionName();
            _collection = Database.GetCollection<T>(_collectionName);
        }
        public async Task InsertAsync(T model)
        {
            await _collection.InsertOneAsync(model);
        }

        public async Task UpdateAsync(T model)
        {
            await _collection.ReplaceOneAsync(c => ((IMongoCollection)c).Id == GetCollectionId(model), model);
        }

        public async Task DeleteAsync(T item)
        {
            await _collection.DeleteOneAsync(c => ((IMongoCollection)c).Id == GetCollectionId(item));
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _collection.Find(t => true).ToListAsync();
        }
         
        public async Task<T> FindById(T model)
        {
            return await _collection.Find(t => ((IMongoCollection)t).Id == GetCollectionId(model)).FirstOrDefaultAsync();
        }

        private static string GetCollectionName() {
            return (typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true).FirstOrDefault() as BsonCollectionAttribute).CollectionName;
        }

        private Guid GetCollectionId(T type)
        {
             return (type as IMongoCollection).Id;
        }
    }
}
