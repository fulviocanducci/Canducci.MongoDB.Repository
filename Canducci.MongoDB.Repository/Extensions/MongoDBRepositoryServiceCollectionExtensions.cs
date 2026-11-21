using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

            RegisterRepositoryInterfaces(services);

            return services;
        }

        private static void RegisterRepositoryInterfaces(IServiceCollection services)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type interfaceType in GetLoadableTypes(assembly))
                {
                    if (!interfaceType.IsInterface || interfaceType.IsGenericTypeDefinition || !interfaceType.IsVisible)
                    {
                        continue;
                    }

                    Type[] inheritedInterfaces = interfaceType.GetInterfaces();
                    if (inheritedInterfaces.Length != 1 || !inheritedInterfaces[0].IsGenericType)
                    {
                        continue;
                    }

                    if (inheritedInterfaces[0].GetGenericTypeDefinition() != typeof(IRepository<>))
                    {
                        continue;
                    }

                    if (interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length != 0)
                    {
                        continue;
                    }

                    Type entityType = inheritedInterfaces[0].GetGenericArguments()[0];

                    services.TryAddScoped(interfaceType, provider =>
                        RepositoryDispatchProxyFactory.Create(
                            interfaceType,
                            entityType,
                            provider.GetRequiredService<IConnect>()));
                }
            }
        }

        private static Type[] GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                return exception.Types.Where(type => type != null).ToArray();
            }
            catch (NotSupportedException)
            {
                return Array.Empty<Type>();
            }
        }
    }
}
