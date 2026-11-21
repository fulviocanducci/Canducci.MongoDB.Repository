using System;
using System.Linq;
using Canducci.MongoDB.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class MongoDBRepositoryServiceCollectionExtensionsTests
    {
        [TestMethod]
        public void AddMongoDBRepository_ShouldRegisterIConnectAsSingleton()
        {
            ServiceCollection services = new ServiceCollection();

            IServiceCollection result = services.AddMongoDBRepository();

            ServiceDescriptor descriptor = result.Single(service => service.ServiceType == typeof(IConnect));

            Assert.AreSame(services, result);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.AreEqual(typeof(Connect), descriptor.ImplementationType);
        }

        [TestMethod]
        public void AddMongoDBRepository_WhenServicesIsNull_ShouldThrowArgumentNullException()
        {
            IServiceCollection services = null;

            Assert.ThrowsException<ArgumentNullException>(() => services.AddMongoDBRepository());
        }
    }
}
