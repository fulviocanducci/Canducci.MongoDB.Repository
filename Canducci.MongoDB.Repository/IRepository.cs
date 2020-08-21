using Canducci.MongoDB.Repository.Paged;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
    public interface IRepository<T>
    {
        #region IMongoColledction
        IMongoCollection<T> Collection { get; }
        #endregion

        #region Add
        T Add(T model);
        IEnumerable<T> Add(IEnumerable<T> models);
        Task<T> AddAsync(T model);
        Task<IEnumerable<T>> AddAsync(IEnumerable<T> models);
        #endregion

        #region Edit
        bool Edit(Expression<Func<T, bool>> filter, T model);
        Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model);
        bool Edit<TKey>(TKey key, T model);
        Task<bool> EditAsync<TKey>(TKey key, T model);
        #endregion

        #region Update
        bool Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update);
        bool UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update);
        Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update);
        Task<bool> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update);
        #endregion

        #region Find
        T Find<TKey>(TKey key);
        T Find(Expression<Func<T, bool>> filter);
        Task<T> FindAsync<TKey>(TKey key);
        Task<T> FindAsync(Expression<Func<T, bool>> filter);
        #endregion

        #region All
        IEnumerable<T> All();
        IEnumerable<T> All(Expression<Func<T, bool>> filter);
        IEnumerable<T> All<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
        Task<IList<T>> AllAsync();
        Task<IList<T>> AllAsync(Expression<Func<T, bool>> filter);
        Task<IList<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
        #endregion

        #region List
        IList<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null);
        Task<IList<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null);
        #endregion

        #region Delete
        bool Delete(Expression<Func<T, bool>> filter);
        Task<bool> DeleteAsync(Expression<Func<T, bool>> filter);
        bool Delete<TKey>(TKey key);
        Task<bool> DeleteAsync<TKey>(TKey key);
        #endregion

        #region IMongoQueryable
        IMongoQueryable<T> Query();
        #endregion

        #region IPagedList
        IPagedList<T> PagedList(int pageNumber, int pageSize);
        Task<IPagedList<T>> PagedListAsync(int pageNumber, int pageSize);
        IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy);
        Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy);
        IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
        Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
        #endregion

        #region CreateObjectId
        ObjectId CreateObjectId(string value);
        #endregion

        #region Count
        long Count();
        long Count(Expression<Func<T, bool>> filter, CountOptions options = null);
        Task<long> CountAsync();
        Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null);
        #endregion
    }
}
