using System;
using System.Collections.Generic;
using System.Linq;
using Canducci.MongoDB.Repository;
using Canducci.MongoDB.Repository.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class MongoDBRepositoryServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddMongoDBRepository_WithConfiguration_ShouldRegisterClientAndConnectAsSingleton()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                ["MongoDB:Database"] = "dbnew",
                ["MongoDB:ConnectionStrings"] = "mongodb://localhost:27017",
                ["MongoDB:Proxy"] = "0"
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            ServiceCollection services = new ServiceCollection();

            IServiceCollection result = services.AddMongoDBRepository(configuration);

            ServiceDescriptor clientDescriptor = result.Single(service => service.ServiceType == typeof(IMongoClient));
            ServiceDescriptor connectDescriptor = result.Single(service => service.ServiceType == typeof(IConnect));

            Assert.AreSame(services, result);
            Assert.AreEqual(ServiceLifetime.Singleton, clientDescriptor.Lifetime);
            Assert.AreEqual(ServiceLifetime.Singleton, connectDescriptor.Lifetime);
            Assert.IsNull(result.SingleOrDefault(service => service.ServiceType == typeof(IRepositoryProxyPerson)));

            using ServiceProvider provider = services.BuildServiceProvider();
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            IConnect connect = provider.GetRequiredService<IConnect>();

            Assert.IsNotNull(connect);
            Assert.AreSame(client, provider.GetRequiredService<IMongoClient>());
        }

        [TestMethod]
        public void AddMongoDBRepository_WithConfigurationProxyEnabled_ShouldRegisterRepositoryProxy()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                ["MongoDB:Database"] = "dbnew",
                ["MongoDB:ConnectionStrings"] = "mongodb://localhost:27017",
                ["MongoDB:Proxy"] = "1"
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            ServiceCollection services = new ServiceCollection();

            services.AddMongoDBRepository(configuration);

            ServiceDescriptor descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(IRepositoryProxyPerson));

            Assert.IsNotNull(descriptor);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [TestMethod]
        public void AddMongoDBRepository_WithInvalidProxyConfiguration_ShouldThrowInvalidOperationException()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                ["MongoDB:Database"] = "dbnew",
                ["MongoDB:ConnectionStrings"] = "mongodb://localhost:27017",
                ["MongoDB:Proxy"] = "2"
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
            ServiceCollection services = new ServiceCollection();

            Assert.Throws<InvalidOperationException>(() => services.AddMongoDBRepository(configuration));
        }

        [TestMethod]
        public void AddMongoDBRepository_WithDatabaseAndConnectionString_ShouldRegisterClientAndConnectAsSingleton()
        {
            ServiceCollection services = new ServiceCollection();

            IServiceCollection result = services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017");

            ServiceDescriptor clientDescriptor = result.Single(service => service.ServiceType == typeof(IMongoClient));
            ServiceDescriptor connectDescriptor = result.Single(service => service.ServiceType == typeof(IConnect));

            Assert.AreSame(services, result);
            Assert.AreEqual(ServiceLifetime.Singleton, clientDescriptor.Lifetime);
            Assert.AreEqual(ServiceLifetime.Singleton, connectDescriptor.Lifetime);

            using ServiceProvider provider = services.BuildServiceProvider();
            Assert.IsNotNull(provider.GetRequiredService<IConnect>());
            Assert.IsNotNull(provider.GetRequiredService<IMongoClient>());
        }

        [TestMethod]
        public void AddMongoDBRepository_WhenServicesIsNull_ShouldThrowArgumentNullException()
        {
            IServiceCollection services = null;

            Assert.Throws<ArgumentNullException>(() =>
                services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017"));
        }

        [TestMethod]
        public void AddMongoDBRepository_WhenConfigurationIsNull_ShouldThrowArgumentNullException()
        {
            ServiceCollection services = new ServiceCollection();
            IConfiguration configuration = null;

            Assert.Throws<ArgumentNullException>(() =>
                services.AddMongoDBRepository(configuration));
        }

        [TestMethod]
        public void AddMongoDBRepository_WhenDatabaseIsMissing_ShouldThrowArgumentException()
        {
            ServiceCollection services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() =>
                services.AddMongoDBRepository(null, "mongodb://localhost:27017"));
        }

        [TestMethod]
        public void AddMongoDBRepository_WhenConnectionStringIsMissing_ShouldThrowArgumentException()
        {
            ServiceCollection services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() =>
                services.AddMongoDBRepository("dbnew", null));
        }
    }
}