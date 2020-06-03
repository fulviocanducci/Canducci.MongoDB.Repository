using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace Canducci.MongoDB.Repository
{
    public sealed class Connect : IConnect
    {
        #region private
        private MongoClient Client { get; set; }
        private IMongoDatabase DataBase { get; set; }
        #endregion

        #region collection
        public IMongoCollection<T> Collection<T>(string CollectionName)
        {
            return Collection<T>(CollectionName, null);
        }
        public IMongoCollection<T> Collection<T>(string CollectionName, MongoCollectionSettings mongoCollectionSettings)
        {
            return DataBase.GetCollection<T>(CollectionName, mongoCollectionSettings);
        }
        #endregion

        #region construct_001
        public Connect(IConfiguration configuration)
            :this(configuration, null) 
        { }
        public Connect(IConfiguration configuration, MongoDatabaseSettings mongoDatabaseSettings)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            IConfigurationSection section = configuration.GetSection("MongoDB");            
            Client = new MongoClient(section["ConnectionStrings"]);
            DataBase = Client.GetDatabase(section["Database"], mongoDatabaseSettings);            
        }
        #endregion

        #region construct_002
        public Connect(string connectionString, string database)
            :this(connectionString, database, null)
        { }
        public Connect(string connectionString, string database, MongoDatabaseSettings mongoDatabaseSettings)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("connectionString is empty", nameof(connectionString));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Database is empty", nameof(database));
            }

            Client = new MongoClient(connectionString);
            DataBase = Client.GetDatabase(database, mongoDatabaseSettings);
        }
        #endregion

        #region construct_003
        public Connect(MongoClientSettings mongoClientSettings, string database):
            this(mongoClientSettings, database, null) 
        { }
        public Connect(MongoClientSettings mongoClientSettings, string database, MongoDatabaseSettings mongoDatabaseSettings)
        {
            if (mongoClientSettings is null)
            {
                throw new ArgumentNullException(nameof(mongoClientSettings));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Database is empty", nameof(database));
            }
            Client = new MongoClient(mongoClientSettings);
            DataBase = Client.GetDatabase(database, mongoDatabaseSettings);
        }
        #endregion

        #region construct_004
        public Connect(MongoUrl mongoUrl, string database)
            :this(mongoUrl, database, null)
        { }
        public Connect(MongoUrl mongoUrl, string database, MongoDatabaseSettings mongoDatabaseSettings = null)
        {
            if (mongoUrl is null)
            {
                throw new ArgumentNullException(nameof(mongoUrl));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("Database is empty", nameof(database));
            }

            Client = new MongoClient(mongoUrl);
            DataBase = Client.GetDatabase(database, mongoDatabaseSettings); 
        }
        #endregion
    }
}
