using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System;

namespace Canducci.MongoDB.Repository
{
    public sealed class Connect : IConnect
    {
        private MongoClient Client { get; set; }
        private IMongoDatabase DataBase { get; set; }
        public IMongoCollection<T> Collection<T>(string CollectionName)
        {
            return DataBase.GetCollection<T>(CollectionName);
        }
        public Connect(IConfiguration configuration)
        {
            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }
            IConfigurationSection section = configuration.GetSection("MongoDB");
            SetMongoClientAndMongoDatabase(section["ConnectionStrings"], section["Database"]);
        }
        public Connect(string connectionString, string database)
        {
            SetMongoClientAndMongoDatabase(connectionString, database);
        }

        private void SetMongoClientAndMongoDatabase(string connectionString, string database)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("message", nameof(connectionString));
            }

            if (string.IsNullOrEmpty(database))
            {
                throw new ArgumentException("message", nameof(database));
            }

            Client = new MongoClient(connectionString);
            DataBase = Client.GetDatabase(database);
        }
    }
}
