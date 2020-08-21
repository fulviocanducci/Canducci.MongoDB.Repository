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

        public T Add(T model)
        {
            Collection.InsertOne(model);
            return model;
        }

        public IEnumerable<T> Add(IEnumerable<T> models)
        {
            Collection.InsertMany(models);
            return models;
        }

        public async Task<T> AddAsync(T model)
        {
            await Collection.InsertOneAsync(model);
            return model;
        }

        public async Task<IEnumerable<T>> AddAsync(IEnumerable<T> models)
        {
            await Collection.InsertManyAsync(models);
            return models;
        }

        #endregion

        #region edit 

        public bool Edit(Expression<Func<T, bool>> filter, T model)
        {
            return Collection
                .ReplaceOne(filter, model)
                .ModifiedCount > 0;
        }

        public async Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model)
        {
            ReplaceOneResult result = await Collection.ReplaceOneAsync(filter, model);
            return result.ModifiedCount > 0;
        }

        #endregion

        #region update

        public bool Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            return Collection.UpdateOne(filter, update).ModifiedCount > 0;
        }

        public bool UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            return Collection.UpdateMany(filter, update).ModifiedCount > 0;
        }

        public async Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            UpdateResult result = await Collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update)
        {
            UpdateResult result = await Collection.UpdateManyAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        #endregion

        #region find
        public T Find<TKey>(TKey key)
        {            
            return Collection.Find(GetFilterDefinition(key)).FirstOrDefault();
        }

        public T Find(Expression<Func<T, bool>> filter)
        {
            return Collection.Find(filter).FirstOrDefault();
        }
        public async Task<T> FindAsync<TKey>(TKey key)
        {      
            IAsyncCursor<T> result = await Collection.FindAsync(GetFilterDefinition(key));
            return await result.FirstOrDefaultAsync();
        }

        public async Task<T> FindAsync(Expression<Func<T, bool>> filter)
        {
            IAsyncCursor<T> result = await Collection.FindAsync(filter);
            return await result.FirstOrDefaultAsync();
        }

        #endregion

        #region all

        public IEnumerable<T> All()
        {
            return Collection.AsQueryable().AsEnumerable();
        }

        public IEnumerable<T> All(Expression<Func<T, bool>> filter)
        {
            return Collection.AsQueryable().Where(filter).AsEnumerable();
        }

        public IEnumerable<T> All<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
        {
            return Collection.AsQueryable().Where(filter).OrderBy(orderBy).AsEnumerable();
        }

        public async Task<IList<T>> AllAsync()
        {
            return await Collection.AsQueryable().ToListAsync();

        }

        public async Task<IList<T>> AllAsync(Expression<Func<T, bool>> filter)
        {
            return await Collection.AsQueryable().Where(filter).ToListAsync();
        }

        public async Task<IList<T>> AllAsync<Tkey>(Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
        {
            return await Collection.AsQueryable().Where(filter).OrderBy(orderBy).ToListAsync();
        }
        #endregion

        #region list

        public IList<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
        {
            IMongoQueryable<T> query = Collection.AsQueryable();
            return (filter != null)
                ? query.Where(filter).OrderBy(orderBy).ToList()
                : query.OrderBy(orderBy).ToList();
        }

        public async Task<IList<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null)
        {
            IMongoQueryable<T> query = Collection.AsQueryable();
            return (filter != null)
                ? await query.Where(filter).OrderBy(orderBy).ToListAsync()
                : await query.OrderBy(orderBy).ToListAsync();
        }

        #endregion

        #region count

        public long Count()
        {
            return Collection.CountDocuments(Builders<T>.Filter.Empty);
        }

        public long Count(Expression<Func<T, bool>> filter, CountOptions options = null)
        {
            return Collection.CountDocuments(filter, options);
        }

        public async Task<long> CountAsync()
        {
            return await Collection.CountDocumentsAsync(Builders<T>.Filter.Empty);
        }

        public async Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null)
        {
            return await Collection.CountDocumentsAsync(filter, options);
        }

        #endregion

        #region delete

        public bool Delete(Expression<Func<T, bool>> filter)
        {
            return Collection.DeleteOne(filter).DeletedCount > 0;
        }

        public async Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)
        {
            DeleteResult result = await Collection.DeleteOneAsync(filter);
            return result.DeletedCount > 0;
        }

        public bool Delete<TKey>(TKey key)
        {               
            return Collection.DeleteOne(GetFilterDefinition(key)).DeletedCount > 0;
        }

        public async Task<bool> DeleteAsync<TKey>(TKey key)
        {                  
            DeleteResult result = await Collection.DeleteOneAsync(GetFilterDefinition(key));
            return result.DeletedCount > 0;
        }

        #endregion

        #region queryAble

        public IMongoQueryable<T> Query()
        {
            return Collection.AsQueryable();
        }

        #endregion

        #region paged
        public IPagedList<T> PagedList(int pageNumber, int pageSize)
        {
            return Query().ToPagedList<T>(pageNumber, pageSize);
        }

        public async Task<IPagedList<T>> PagedListAsync(int pageNumber, int pageSize)
            => await Task.FromResult(PagedList(pageNumber, pageSize));

        public IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy)
        {
            return Query().OrderBy(orderBy).ToPagedList<T>(pageNumber, pageSize);
        }

        public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy)
            => await Task.FromResult(PagedList<Tkey>(pageNumber, pageSize, orderBy));

        public IPagedList<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
        {
            return Query().Where(filter).OrderBy(orderBy).ToPagedList<T>(pageNumber, pageSize);
        }
        public async Task<IPagedList<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy)
            => await Task.FromResult(PagedList<Tkey>(pageNumber, pageSize, filter, orderBy));
        #endregion

        #region objectId

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

            var id = typeInfo.GetProperties()
                .Where(w => w.GetCustomAttribute(typeof(BsonIdAttribute)).GetType() == typeof(BsonIdAttribute)).FirstOrDefault();
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
