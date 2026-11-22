using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Reflection;

namespace Canducci.MongoDB.Repository.DependencyInjection
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
            string proxyValue = section["Proxy"];

            RepositoryRegistrationMode registrationMode = RepositoryRegistrationMode.Manual;
            if (!string.IsNullOrWhiteSpace(proxyValue))
            {
                if (!int.TryParse(proxyValue, out int proxy) ||
                    !Enum.IsDefined(typeof(RepositoryRegistrationMode), proxy))
                {
                    throw new InvalidOperationException("The MongoDB:Proxy configuration value must be 0 (Manual) or 1 (Proxy).");
                }

                registrationMode = (RepositoryRegistrationMode)proxy;
            }

            return AddMongoDBRepository(services, database, connectionString, registrationMode);
        }

        public static IServiceCollection AddMongoDBRepository(
            this IServiceCollection services,
            Action<MongoDBRepositoryConfiguration> configure)
        {
            if (services is null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (configure is null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            MongoDBRepositoryConfiguration configuration = new MongoDBRepositoryConfiguration();
            configure(configuration);

            if (!Enum.IsDefined(typeof(RepositoryRegistrationMode), configuration.RegistrationMode))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(configuration.RegistrationMode),
                    "Registration mode must be Manual (0) or Proxy (1).");
            }

            return AddMongoDBRepository(
                services,
                configuration.Database,
                configuration.ConnectionString,
                configuration.RegistrationMode);
        }

        public static IServiceCollection AddMongoDBRepository(
            this IServiceCollection services,
            string database,
            string connectionString)
        {
            return AddMongoDBRepository(
                services,
                database,
                connectionString,
                RepositoryRegistrationMode.Manual);
        }

        private static IServiceCollection AddMongoDBRepository(
            IServiceCollection services,
            string database,
            string connectionString,
            RepositoryRegistrationMode registrationMode)
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

            if (!Enum.IsDefined(typeof(RepositoryRegistrationMode), registrationMode))
            {
                throw new ArgumentOutOfRangeException(nameof(registrationMode));
            }

            IMongoClient mongoClient = new MongoClient(connectionString);

            services.AddSingleton(mongoClient);
            services.AddSingleton<IConnect>(provider =>
                new Connect(provider.GetRequiredService<IMongoClient>(), database));

            if (registrationMode == RepositoryRegistrationMode.Proxy)
            {
                RegisterRepositoryInterfaces(services);
            }

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

                    Type repositoryInterface = interfaceType.GetInterfaces()
                        .FirstOrDefault(inheritedInterface =>
                            inheritedInterface.IsGenericType &&
                            inheritedInterface.GetGenericTypeDefinition() == typeof(IRepository<>));

                    if (repositoryInterface is null)
                    {
                        continue;
                    }

                    if (interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Length != 0)
                    {
                        continue;
                    }

                    Type entityType = repositoryInterface.GetGenericArguments()[0];

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