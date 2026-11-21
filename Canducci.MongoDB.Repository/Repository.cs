using Canducci.MongoDB.Repository.Attributes;
using Canducci.MongoDB.Repository.Paged;
using Canducci.MongoDB.Repository.Utils;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
   public abstract class Repository<T> : IRepository<T> where T : class, new()
   {

      #region protected
      protected string BsonIdName { get; private set; }
      protected IConnect Connect { get; private set; }
      protected string CollectionName { get; private set; }
      #endregion

      #region public
      /// <summary>
      /// MongoCollection
      /// </summary>
      public IMongoCollection<T> Collection { get; private set; }
      #endregion

      #region construct        
      public Repository(IConnect connect)
          : this(connect, null)
      { }
      public Repository(IConnect connect, MongoCollectionSettings mongoCollectionSettings)
      {
         CollectionName = RepositoryUtil.GetCollectionName<T>();
         BsonIdName = RepositoryUtil.GetBsonIdName<T>();
         Connect = connect;
         Collection = RepositoryUtil.GetCollection<T>(Connect, CollectionName, mongoCollectionSettings);
      }
      #endregion

      #region add 
      /// <summary>
      /// Insert single document
      /// </summary>
      /// <param name="model">Collection</param>
      /// <returns>Document</returns>
      public T Add(T model, CancellationToken cancellationToken = default)
      {
         Collection.InsertOne(model, cancellationToken: cancellationToken);
         return model;
      }
      /// <summary>
      /// Insert many documents
      /// </summary>
      /// <param name="models">Collections</param>
      /// <returns>Document</returns>
      public IEnumerable<T> Add(IEnumerable<T> models, CancellationToken cancellationToken = default)
      {
         Collection.InsertMany(models, cancellationToken: cancellationToken);
         return models;
      }
      /// <summary>
      /// Insert single document
      /// </summary>
      /// <param name="model">Collection</param>
      /// <returns>Document</returns>
      public async Task<T> AddAsync(T model, CancellationToken cancellationToken = default)
      {
         await Collection.InsertOneAsync(model, cancellationToken: cancellationToken);
         return model;
      }
      /// <summary>
      /// Insert many documents
      /// </summary>
      /// <param name="models">Collections</param>
      /// <returns>Document</returns>
      public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> models, CancellationToken cancellationToken = default)
      {
         await Collection.InsertManyAsync(models, cancellationToken: cancellationToken);
         return models;
      }

      #endregion

      #region edit 
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public bool Edit<TKey>(TKey key, T model, CancellationToken cancellationToken = default)
      {
         return Collection
             .ReplaceOne(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key), model, cancellationToken: cancellationToken)
             .ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public async Task<bool> EditAsync<TKey>(TKey key, T model, CancellationToken cancellationToken = default)
      {
         ReplaceOneResult result = await Collection
             .ReplaceOneAsync(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key), model, cancellationToken: cancellationToken);
         return result.ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public bool Edit(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default)
      {
         return Collection
             .ReplaceOne(filter, model, cancellationToken: cancellationToken)
             .ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public async Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default)
      {
         ReplaceOneResult result = await Collection
             .ReplaceOneAsync(filter, model, cancellationToken: cancellationToken);
         return result.ModifiedCount > 0;
      }

      #endregion

      #region update
      /// <summary>
      /// Updates a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public bool Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return Collection.UpdateOne(filter, update, cancellationToken: cancellationToken).ModifiedCount > 0;
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public bool UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return Collection.UpdateMany(filter, update, cancellationToken: cancellationToken).ModifiedCount > 0;
      }
      /// <summary>
      /// Updates a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public async Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         UpdateResult result = await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
         return result.ModifiedCount > 0;
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public async Task<bool> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         UpdateResult result = await Collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
         return result.ModifiedCount > 0;
      }

      #endregion

      #region find
      /// <summary>
      /// Find
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>Document</returns>
      public T Find<TKey>(TKey key, CancellationToken cancellationToken = default)
      {
         return Collection.Find(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key)).FirstOrDefault(cancellationToken);
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>Document</returns>
      public T Find(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
      {
         return Collection.Find(filter).FirstOrDefault(cancellationToken);
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>Document</returns>
      public async Task<T> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
      {
         IAsyncCursor<T> result = await Collection.FindAsync(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key));
         return await result.FirstOrDefaultAsync(cancellationToken);
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <param name="filter">Parameter Filter</param>
      /// <returns>Document</returns>
      public async Task<T> FindAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
      {
         IAsyncCursor<T> result = await Collection.FindAsync(filter, cancellationToken: cancellationToken);
         return await result.FirstOrDefaultAsync(cancellationToken);
      }

      #endregion

      #region all
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <returns>IEnumerable Documents</returns>
      public IEnumerable<T> All()
      {
         return Collection.AsQueryable().AsEnumerable();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>IEnumerable Documents</returns>
      public IEnumerable<T> All(Expression<Func<T, bool>> filter)
      {
         return Collection.AsQueryable().Where(filter).AsEnumerable();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IEnumerable Document</returns>
      public IEnumerable<T> All<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return Collection.AsQueryable().Where(filter).OrderBy(orderBy).AsEnumerable();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <returns>IEnumerable Documents</returns>
      public Task<IEnumerable<T>> AllAsync()
      {
         return AllAsync(CancellationToken.None);
      }
      public async Task<IEnumerable<T>> AllAsync(CancellationToken cancellationToken)
      {
         return await Collection.Find(Builders<T>.Filter.Empty).ToListAsync(cancellationToken);
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>IEnumerable Documents</returns>
      public Task<IEnumerable<T>> AllAsync(Expression<Func<T, bool>> filter)
      {
         return AllAsync(filter, CancellationToken.None);
      }
      public async Task<IEnumerable<T>> AllAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken)
      {
         return await Collection.Find(filter).ToListAsync(cancellationToken);
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IEnumerable Document</returns>
      public Task<IEnumerable<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return AllAsync(filter, orderBy, CancellationToken.None);
      }
      public async Task<IEnumerable<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken)
      {
         return await Collection.Find(filter).SortBy(orderBy).ToListAsync(cancellationToken);
      }
      #endregion

      #region list
      /// <summary>
      /// List documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <param name="filter">`Parameter filter</param>
      /// <returns>List documents</returns>
      public IEnumerable<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
      {
         IQueryable<T> query = Collection.AsQueryable();
         return (filter != null)
             ? query.Where(filter).OrderBy(orderBy).AsEnumerable()
             : query.OrderBy(orderBy).AsEnumerable();
      }
      /// <summary>
      /// List documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <param name="filter">`Parameter filter</param>
      /// <returns>List documents</returns>
      public Task<IEnumerable<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
      {
         return ListAsync(orderBy, filter, CancellationToken.None);
      }
      public async Task<IEnumerable<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter, CancellationToken cancellationToken)
      {
         IFindFluent<T, T> query = filter != null
             ? Collection.Find(filter)
             : Collection.Find(Builders<T>.Filter.Empty);
         return await query.SortBy(orderBy).ToListAsync(cancellationToken);
      }
      #endregion

      #region count
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <returns>long</returns>
      public long Count(CancellationToken cancellationToken = default)
      {
         return Collection.CountDocuments(Builders<T>.Filter.Empty, cancellationToken: cancellationToken);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="options">Parameter CountOptions</param>
      /// <returns>long</returns>
      public long Count(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default)
      {
         return Collection.CountDocuments(filter, options, cancellationToken);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <returns>long</returns>
      public async Task<long> CountAsync(CancellationToken cancellationToken = default)
      {
         return await Collection.CountDocumentsAsync(Builders<T>.Filter.Empty, cancellationToken: cancellationToken);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="options">Parameter CountOptions</param>
      /// <returns>long</returns>
      public async Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default)
      {
         return await Collection.CountDocumentsAsync(filter, options, cancellationToken);
      }

      #endregion

      #region delete
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>bool</returns>
      public bool Delete(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
      {
         return Collection.DeleteOne(filter, cancellationToken).DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>bool</returns>
      public async Task<bool> DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
      {
         DeleteResult result = await Collection.DeleteOneAsync(filter, cancellationToken);
         return result.DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>bool</returns>
      public bool Delete<TKey>(TKey key, CancellationToken cancellationToken = default)
      {
         return Collection.DeleteOne(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key), cancellationToken).DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>bool</returns>
      public async Task<bool> DeleteAsync<TKey>(TKey key, CancellationToken cancellationToken = default)
      {
         DeleteResult result = await Collection.DeleteOneAsync(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key), cancellationToken);
         return result.DeletedCount > 0;
      }

      #endregion

      #region queryAble
      /// <summary>
      /// Queryable source
      /// </summary>
      /// <returns></returns>
      public IQueryable<T> Query()
      {
         return Collection.AsQueryable();
      }

      #endregion

      #region paged
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <returns>IPagedList</returns>
      public IPagedList<T> PagedList(int pageNumber, int pageSize)
      {
         return Query().ToPagedList<T>(pageNumber, pageSize);
      }
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <returns>IPagedList</returns>
      public Task<IPagedList<T>> PagedListAsync(int pageNumber, int pageSize)
      {
         return PagedListAsync(pageNumber, pageSize, CancellationToken.None);
      }
      public async Task<IPagedList<T>> PagedListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
      {
         return await CreatePagedListAsync(Builders<T>.Filter.Empty, null, pageNumber, pageSize, cancellationToken);
      }
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IPagedList</returns>
      public IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy)
      {
         return Query().OrderBy(orderBy).ToPagedList<T>(pageNumber, pageSize);
      }
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IPagedList</returns>
      public Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy)
      {
         return PagedListAsync(pageNumber, pageSize, orderBy, CancellationToken.None);
      }
      public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken)
      {
         return await CreatePagedListAsync(
             Builders<T>.Filter.Empty,
             Builders<T>.Sort.Ascending(orderBy),
             pageNumber,
             pageSize,
             cancellationToken);
      }
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IPagedList</returns>
      public IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return Query().Where(filter).OrderBy(orderBy).ToPagedList<T>(pageNumber, pageSize);
      }
      /// <summary>
      /// Paging documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="pageNumber">Page number</param>
      /// <param name="pageSize">Page size</param>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IPagedList</returns>
      public Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return PagedListAsync(pageNumber, pageSize, filter, orderBy, CancellationToken.None);
      }
      public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken)
      {
         return await CreatePagedListAsync(
             Builders<T>.Filter.Where(filter),
             Builders<T>.Sort.Ascending(orderBy),
             pageNumber,
             pageSize,
             cancellationToken);
      }
      private async Task<IPagedList<T>> CreatePagedListAsync(
          FilterDefinition<T> filter,
          SortDefinition<T> sort,
          int pageNumber,
          int pageSize,
          CancellationToken cancellationToken)
      {
         if (pageNumber < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber cannot be below 1.");
         }
         if (pageSize < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
         }

         long totalItemCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
         IFindFluent<T, T> query = Collection.Find(filter);
         if (sort != null)
         {
            query = query.Sort(sort);
         }

         List<T> items = await query
             .Skip((pageNumber - 1) * pageSize)
             .Limit(pageSize)
             .ToListAsync(cancellationToken);

         return new PagedList<T>(items, pageNumber, pageSize, checked((int)totalItemCount));
      }

      #endregion

      #region objectId
      /// <summary>
      /// Parse string and create new ObjectId
      /// </summary>
      /// <param name="value">Parameter value</param>
      /// <returns>ObjectId</returns>
      public ObjectId CreateObjectId(string value)
      {
         return ObjectId.Parse(value);
      }

      #endregion

   }
}
