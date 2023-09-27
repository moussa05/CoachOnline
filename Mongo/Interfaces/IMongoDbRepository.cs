using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoachOnline.Mongo.Interfaces
{
    public interface IMongoDbRepository<T> where T : class
    {
        Task InsertAsync(T model);
        Task<List<T>> GetAllAsync();
        Task UpdateAsync(T model);
        Task DeleteAsync(T model);
        Task<T> FindById(T model);
    }
}
