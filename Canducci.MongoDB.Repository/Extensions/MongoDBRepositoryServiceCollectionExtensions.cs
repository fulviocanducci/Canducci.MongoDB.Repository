using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository
{
    public static class MongoDBRepositoryServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoDBRepository(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configuration is null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            IConfigurationSection section = configuration.GetSection("MongoDB");
            string database = section["Database"];
            string connectionString = section["ConnectionStrings"];

            return AddMongoDBRepository(services, database, connectionString);
        }

        public static IServiceCollection AddMongoDBRepository(
            this IServiceCollection services,
            string database,
            string connectionString)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new ArgumentException("Database is required.", nameof(database));
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            IMongoClient mongoClient = new MongoClient(connectionString);

            services.AddSingleton(mongoClient);
            services.AddSingleton<IConnect>(provider =>
                new Connect(provider.GetRequiredService<IMongoClient>(), database));

            return services;
        }
    }
}