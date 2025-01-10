using Canducci.MongoDB.Repository.Attributes;
using Canducci.MongoDB.Repository.Paged;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
         SetConnectAndCollectionAndBsonId(connect, mongoCollectionSettings);
      }
      #endregion

      #region add 
      /// <summary>
      /// Insert single document
      /// </summary>
      /// <param name="model">Collection</param>
      /// <returns>Document</returns>
      public T Add(T model)
      {
         Collection.InsertOne(model);
         return model;
      }
      /// <summary>
      /// Insert many documents
      /// </summary>
      /// <param name="models">Collections</param>
      /// <returns>Document</returns>
      public IEnumerable<T> Add(IEnumerable<T> models)
      {
         Collection.InsertMany(models);
         return models;
      }
      /// <summary>
      /// Insert single document
      /// </summary>
      /// <param name="model">Collection</param>
      /// <returns>Document</returns>
      public async Task<T> AddAsync(T model)
      {
         await Collection.InsertOneAsync(model);
         return model;
      }
      /// <summary>
      /// Insert many documents
      /// </summary>
      /// <param name="models">Collections</param>
      /// <returns>Document</returns>
      public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> models)
      {
         await Collection.InsertManyAsync(models);
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
      public bool Edit<TKey>(TKey key, T model)
      {
         return Collection
             .ReplaceOne(GetFilterDefinition<TKey>(key), model)
             .ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public async Task<bool> EditAsync<TKey>(TKey key, T model)
      {
         ReplaceOneResult result = await Collection
             .ReplaceOneAsync(GetFilterDefinition<TKey>(key), model);
         return result.ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public bool Edit(Expression<Func<T, bool>> filter, T model)
      {
         return Collection
             .ReplaceOne(filter, model)
             .ModifiedCount > 0;
      }
      /// <summary>
      /// Replaces a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="model">Collection</param>
      /// <returns>bool</returns>
      public async Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model)
      {
         ReplaceOneResult result = await Collection
             .ReplaceOneAsync(filter, model);
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
      public bool Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
      {
         return Collection.UpdateOne(filter, update).ModifiedCount > 0;
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public bool UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
      {
         return Collection.UpdateMany(filter, update).ModifiedCount > 0;
      }
      /// <summary>
      /// Updates a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public async Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
      {
         UpdateResult result = await Collection.UpdateOneAsync(filter, update);
         return result.ModifiedCount > 0;
      }
      /// <summary>
      /// Updates many documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="update">Base class for updates</param>
      /// <returns>bool</returns>
      public async Task<bool> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
      {
         UpdateResult result = await Collection.UpdateManyAsync(filter, update);
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
      public T Find<TKey>(TKey key)
      {
         return Collection.Find(GetFilterDefinition(key)).FirstOrDefault();
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>Document</returns>
      public T Find(Expression<Func<T, bool>> filter)
      {
         return Collection.Find(filter).FirstOrDefault();
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>Document</returns>
      public async Task<T> FindAsync<TKey>(TKey key)
      {
         IAsyncCursor<T> result = await Collection.FindAsync(GetFilterDefinition(key));
         return await result.FirstOrDefaultAsync();
      }
      /// <summary>
      /// Find
      /// </summary>
      /// <param name="filter">Parameter Filter</param>
      /// <returns>Document</returns>
      public async Task<T> FindAsync(Expression<Func<T, bool>> filter)
      {
         IAsyncCursor<T> result = await Collection.FindAsync(filter);
         return await result.FirstOrDefaultAsync();
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
      public async Task<IList<T>> AllAsync()
      {
         return await Collection.AsQueryable().ToListAsync();

      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>IEnumerable Documents</returns>
      public async Task<IList<T>> AllAsync(Expression<Func<T, bool>> filter)
      {
         return await Collection.AsQueryable().Where(filter).ToListAsync();
      }
      /// <summary>
      /// IEnumerable Documents
      /// </summary>
      /// <typeparam name="Tkey">Parameter type</typeparam>
      /// <param name="filter">Parameter filter</param>
      /// <param name="orderBy">Parameter orderBy</param>
      /// <returns>IEnumerable Document</returns>
      public async Task<IList<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return await Collection.AsQueryable().Where(filter).OrderBy(orderBy).ToListAsync();
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
      public IList<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
      {
         IMongoQueryable<T> query = Collection.AsQueryable();
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
      public async Task<IList<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
      {
         IMongoQueryable<T> query = Collection.AsQueryable();
         return (filter != null)
             ? await query.Where(filter).OrderBy(orderBy).ToListAsync()
             : await query.OrderBy(orderBy).ToListAsync();
      }

      #endregion

      #region count
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <returns>long</returns>
      public long Count()
      {
         return Collection.CountDocuments(Builders<T>.Filter.Empty);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="options">Parameter CountOptions</param>
      /// <returns>long</returns>
      public long Count(Expression<Func<T, bool>> filter, CountOptions options = null)
      {
         return Collection.CountDocuments(filter, options);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <returns>long</returns>
      public async Task<long> CountAsync()
      {
         return await Collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
      }
      /// <summary>
      /// Returns number of documents in the collection
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <param name="options">Parameter CountOptions</param>
      /// <returns>long</returns>
      public async Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null)
      {
         return await Collection.CountDocumentsAsync(filter, options);
      }

      #endregion

      #region delete
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>bool</returns>
      public bool Delete(Expression<Func<T, bool>> filter)
      {
         return Collection.DeleteOne(filter).DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <param name="filter">Parameter filter</param>
      /// <returns>bool</returns>
      public async Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)
      {
         DeleteResult result = await Collection.DeleteOneAsync(filter);
         return result.DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>bool</returns>
      public bool Delete<TKey>(TKey key)
      {
         return Collection.DeleteOne(GetFilterDefinition(key)).DeletedCount > 0;
      }
      /// <summary>
      /// Delete a single document
      /// </summary>
      /// <typeparam name="TKey">Parameter type</typeparam>
      /// <param name="key">Parameter value</param>
      /// <returns>bool</returns>
      public async Task<bool> DeleteAsync<TKey>(TKey key)
      {
         DeleteResult result = await Collection.DeleteOneAsync(GetFilterDefinition(key));
         return result.DeletedCount > 0;
      }

      #endregion

      #region queryAble
      /// <summary>
      /// Queryable source
      /// </summary>
      /// <returns></returns>
      public IMongoQueryable<T> Query()
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
      public async Task<IPagedList<T>> PagedListAsync(int pageNumber, int pageSize)
      {
         return await Task.FromResult(PagedList(pageNumber, pageSize));
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
      public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy)
      {
         return await Task.FromResult(PagedList<Tkey>(pageNumber, pageSize, orderBy));
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
      public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
      {
         return await Task.FromResult(PagedList<Tkey>(pageNumber, pageSize, filter, orderBy));
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

      #region Internal
      internal void SetCollectionNameAndBsonId()
      {
         TypeInfo typeInfo = typeof(T).GetTypeInfo();

         BsonCollectionName bsonCollectionName = (BsonCollectionName)typeInfo.GetCustomAttribute(typeof(BsonCollectionName));
         CollectionName = bsonCollectionName != null ? bsonCollectionName.Name : typeof(T).Name.ToLower();

         var id = typeInfo.GetProperties().Where(w => w.GetCustomAttribute(typeof(BsonIdAttribute)).GetType() == typeof(BsonIdAttribute)).FirstOrDefault();
         BsonIdName = id?.Name;
      }
      internal void SetConnectAndCollectionAndBsonId(IConnect connect, MongoCollectionSettings mongoCollectionSettings)
      {
         SetCollectionNameAndBsonId();
         Connect = connect;
         Collection = Connect.Collection<T>(CollectionName, mongoCollectionSettings);
      }
      internal FilterDefinition<T> GetFilterDefinition<TKey>(TKey key)
      {
         if (string.IsNullOrEmpty(BsonIdName))
         {
            throw new ArgumentException($"'{nameof(BsonIdName)}' cannot be null or empty", nameof(BsonIdName));
         }
         StringFieldDefinition<T, TKey> field = new StringFieldDefinition<T, TKey>(BsonIdName);
         return Builders<T>.Filter.Eq<TKey>(field, key);
      }
      #endregion
   }
}
