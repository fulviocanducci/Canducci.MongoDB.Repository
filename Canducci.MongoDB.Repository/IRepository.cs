using Canducci.MongoDB.Repository.Paged;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
   public interface IRepository<T>
   {
      #region IMongoColledction
      IMongoCollection<T> Collection { get; }
      #endregion

      #region Add
      T Add(T model, CancellationToken cancellationToken = default);
      IEnumerable<T> Add(IEnumerable<T> models, CancellationToken cancellationToken = default);
      Task<T> AddAsync(T model, CancellationToken cancellationToken = default);
      Task<IEnumerable<T>> AddAsync(IEnumerable<T> models, CancellationToken cancellationToken = default);
      #endregion

      #region Edit
      bool Edit(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default);
      Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default);
      bool Edit<TKey>(TKey key, T model, CancellationToken cancellationToken = default);
      Task<bool> EditAsync<TKey>(TKey key, T model, CancellationToken cancellationToken = default);
      #endregion

      #region Update
      bool Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
      bool UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
      Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
      Task<bool> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
      #endregion

      #region Find
      T Find<TKey>(TKey key, CancellationToken cancellationToken = default);
      T Find(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
      Task<T> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default);
      Task<T> FindAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
      #endregion

      #region All
      IEnumerable<T> All();
      IEnumerable<T> All(Expression<Func<T, bool>> filter);
      IEnumerable<T> All<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
      Task<IEnumerable<T>> AllAsync();
      Task<IEnumerable<T>> AllAsync(Expression<Func<T, bool>> filter);
      Task<IEnumerable<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy);
      #endregion

      #region List
      IEnumerable<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null);
      Task<IEnumerable<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null);
      #endregion

      #region Delete
      bool Delete(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
      Task<bool> DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
      bool Delete<TKey>(TKey key, CancellationToken cancellationToken = default);
      Task<bool> DeleteAsync<TKey>(TKey key, CancellationToken cancellationToken = default);
      #endregion

      #region IQueryable
      IQueryable<T> Query();
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
      long Count(CancellationToken cancellationToken = default);
      long Count(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default);
      Task<long> CountAsync(CancellationToken cancellationToken = default);
      Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default);
      #endregion
   }
}
