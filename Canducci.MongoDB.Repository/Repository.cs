using Canducci.MongoDB.Repository.Attributes;
using Canducci.Pagination.Interfaces;
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
      public IReadOnlyList<T> Add(IEnumerable<T> models, CancellationToken cancellationToken = default)
      {
         List<T> items = models.ToList();
         Collection.InsertMany(items, cancellationToken: cancellationToken);
         return items;
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
      public async Task<IReadOnlyList<T>> AddAsync(IEnumerable<T> models, CancellationToken cancellationToken = default)
      {
         List<T> items = models.ToList();
         await Collection.InsertManyAsync(items, cancellationToken: cancellationToken);
         return items;
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
             .MatchedCount > 0;
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
         return result.MatchedCount > 0;
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
             .MatchedCount > 0;
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
         return result.MatchedCount > 0;
      }

      #endregion

      #region update
      /// <summary>
      /// Updates a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>UpdateResult</returns>
      public UpdateResult Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return Collection.UpdateOne(filter, update, cancellationToken: cancellationToken);
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>UpdateResult</returns>
      public UpdateResult UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return Collection.UpdateMany(filter, update, cancellationToken: cancellationToken);
      }
      /// <summary>
      /// Updates a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>UpdateResult</returns>
      public async Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return await Collection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>UpdateResult</returns>
      public async Task<UpdateResult> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default)
      {
         return await Collection.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
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
         IAsyncCursor<T> result = await Collection.FindAsync(RepositoryUtil.GetFilterDefinition<T, TKey>(BsonIdName, key), cancellationToken: cancellationToken);
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
      public IReadOnlyList<T> All()
      {
         return Collection.AsQueryable().ToList();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>IEnumerable Documents</returns>
      public IReadOnlyList<T> All(Expression<Func<T, bool>> filter)
      {
         return Collection.AsQueryable().Where(filter).ToList();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IEnumerable Document</returns>
      public IReadOnlyList<T> All<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return Collection.AsQueryable().Where(filter).OrderBy(orderBy).ToList();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <returns>IEnumerable Documents</returns>
      public async Task<IReadOnlyList<T>> AllAsync(CancellationToken cancellationToken = default)
      {
         return await Collection.Find(Builders<T>.Filter.Empty).ToListAsync(cancellationToken);
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>IEnumerable Documents</returns>
      public async Task<IReadOnlyList<T>> AllAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default)
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
      public async Task<IReadOnlyList<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default)
      {
         FieldDefinition<T> field = new ExpressionFieldDefinition<T, Tkey>(orderBy);
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(field);
         return await Collection.Find(filter).Sort(sort).ToListAsync(cancellationToken);
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
      public IReadOnlyList<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
      {
         IQueryable<T> query = Collection.AsQueryable();
         return (filter != null)
             ? query.Where(filter).OrderBy(orderBy).ToList()
             : query.OrderBy(orderBy).ToList();
      }
      /// <summary>
      /// List documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <param name="filter">`Parameter filter</param>
      /// <returns>List documents</returns>
      public async Task<IReadOnlyList<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null, CancellationToken cancellationToken = default)
      {
         IFindFluent<T, T> query = filter != null
             ? Collection.Find(filter)
             : Collection.Find(Builders<T>.Filter.Empty);
         FieldDefinition<T> field = new ExpressionFieldDefinition<T, Tkey>(orderBy);
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(field);
         return await query.Sort(sort).ToListAsync(cancellationToken);
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
      /// Returns a page of documents.
      /// </summary>
      public IPaginated<T> PagedList(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
      {
         return CreatePagedList(Builders<T>.Filter.Empty, null, pageNumber, pageSize, cancellationToken);
      }

      /// <summary>
      /// Returns a page of documents asynchronously.
      /// </summary>
      public Task<IPaginated<T>> PagedListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
      {
         return CreatePagedListAsync(Builders<T>.Filter.Empty, null, pageNumber, pageSize, cancellationToken);
      }

      /// <summary>
      /// Returns an ordered page of documents.
      /// </summary>
      public IPaginated<T> PagedList<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, CancellationToken cancellationToken = default)
      {
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(new ExpressionFieldDefinition<T, TKey>(orderBy));
         return CreatePagedList(Builders<T>.Filter.Empty, sort, pageNumber, pageSize, cancellationToken);
      }

      /// <summary>
      /// Returns an ordered page of documents asynchronously.
      /// </summary>
      public Task<IPaginated<T>> PagedListAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, TKey>> orderBy, CancellationToken cancellationToken = default)
      {
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(new ExpressionFieldDefinition<T, TKey>(orderBy));
         return CreatePagedListAsync(Builders<T>.Filter.Empty, sort, pageNumber, pageSize, cancellationToken);
      }

      /// <summary>
      /// Returns a filtered and ordered page of documents.
      /// </summary>
      public IPaginated<T> PagedList<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderBy, CancellationToken cancellationToken = default)
      {
         FilterDefinition<T> filterDefinition = Builders<T>.Filter.Where(filter);
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(new ExpressionFieldDefinition<T, TKey>(orderBy));
         return CreatePagedList(filterDefinition, sort, pageNumber, pageSize, cancellationToken);
      }

      /// <summary>
      /// Returns a filtered and ordered page of documents asynchronously.
      /// </summary>
      public Task<IPaginated<T>> PagedListAsync<TKey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderBy, CancellationToken cancellationToken = default)
      {
         FilterDefinition<T> filterDefinition = Builders<T>.Filter.Where(filter);
         SortDefinition<T> sort = Builders<T>.Sort.Ascending(new ExpressionFieldDefinition<T, TKey>(orderBy));
         return CreatePagedListAsync(filterDefinition, sort, pageNumber, pageSize, cancellationToken);
      }

      private IPaginated<T> CreatePagedList(
          FilterDefinition<T> filter,
          SortDefinition<T> sort,
          int pageNumber,
          int pageSize,
          CancellationToken cancellationToken)
      {
         ValidatePageArguments(pageNumber, pageSize);
         long totalItemCount = Collection.CountDocuments(filter, cancellationToken: cancellationToken);
         IFindFluent<T, T> query = Collection.Find(filter);
         if (sort != null)
         {
            query = query.Sort(sort);
         }

         List<T> items = query
             .Skip(checked((pageNumber - 1) * pageSize))
             .Limit(pageSize)
             .ToList(cancellationToken);

         return new PagedList<T>(items, pageNumber, pageSize, checked((int)totalItemCount));
      }

      private async Task<IPaginated<T>> CreatePagedListAsync(
          FilterDefinition<T> filter,
          SortDefinition<T> sort,
          int pageNumber,
          int pageSize,
          CancellationToken cancellationToken)
      {
         ValidatePageArguments(pageNumber, pageSize);
         long totalItemCount = await Collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
         IFindFluent<T, T> query = Collection.Find(filter);
         if (sort != null)
         {
            query = query.Sort(sort);
         }

         List<T> items = await query
             .Skip(checked((pageNumber - 1) * pageSize))
             .Limit(pageSize)
             .ToListAsync(cancellationToken);

         return new PagedList<T>(items, pageNumber, pageSize, checked((int)totalItemCount));
      }

      private static void ValidatePageArguments(int pageNumber, int pageSize)
      {
         if (pageNumber < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber cannot be below 1.");
         }
         if (pageSize < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
         }
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
