using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System;
using System.Reflection;

namespace Canducci.MongoDB.Repository.Utils
{
    internal static class RepositoryUtil
    {
        public static string GetCollectionName<T>()
        {
            TypeInfo typeInfo = typeof(T).GetTypeInfo();
            BsonCollectionName bsonCollectionName = (BsonCollectionName)typeInfo.GetCustomAttribute(typeof(BsonCollectionName));
            return bsonCollectionName != null ? bsonCollectionName.Name : typeof(T).Name.ToLower();
        }

        public static string GetBsonIdName<T>()
        {
            BsonClassMap classMap = BsonClassMap.LookupClassMap(typeof(T));
            return classMap.IdMemberMap?.MemberName;
        }

        public static IMongoCollection<T> GetCollection<T>(IConnect connect, string collectionName, MongoCollectionSettings mongoCollectionSettings)
        {
            return connect.Collection<T>(collectionName, mongoCollectionSettings);
        }

        public static FilterDefinition<T> GetFilterDefinition<T, TKey>(string bsonIdName, TKey key)
        {
            if (string.IsNullOrEmpty(bsonIdName))
            {
                throw new ArgumentException($"'{nameof(bsonIdName)}' cannot be null or empty", nameof(bsonIdName));
            }

            StringFieldDefinition<T, TKey> field = new StringFieldDefinition<T, TKey>(bsonIdName);
            return Builders<T>.Filter.Eq<TKey>(field, key);
        }
    }
}
