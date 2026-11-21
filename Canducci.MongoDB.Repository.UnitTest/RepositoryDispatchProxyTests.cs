using System.Linq;
using Canducci.MongoDB.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    public class ProxyPerson
    {
        public string Id { get; set; }
    }

    public interface IRepositoryProxyPerson : IRepository<ProxyPerson>
    {
    }

    public interface IRepositoryProxyPersonWithCustomMethod : IRepository<ProxyPerson>
    {
        string GetDisplayName();
    }

    [TestClass]
    public class RepositoryDispatchProxyTests
    {
        [TestMethod]
        public void AddMongoDBRepository_ShouldRegisterEmptyRepositoryInterfaceAsScopedProxy()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017");

            ServiceDescriptor descriptor = services.SingleOrDefault(
                service => service.ServiceType == typeof(IRepositoryProxyPerson));

            Assert.IsNotNull(descriptor);
            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.Lifetime);

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            IRepositoryProxyPerson first = scope.ServiceProvider.GetRequiredService<IRepositoryProxyPerson>();
            IRepositoryProxyPerson second = scope.ServiceProvider.GetRequiredService<IRepositoryProxyPerson>();

            Assert.IsNotNull(first);
            Assert.AreSame(first, second);
            Assert.IsNotNull(first.Collection);
            Assert.IsTrue(first is IRepository<ProxyPerson>);
        }

        [TestMethod]
        public void AddMongoDBRepository_ShouldNotRegisterRepositoryInterfaceWithCustomMethods()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddMongoDBRepository("dbnew", "mongodb://localhost:27017");

            using ServiceProvider provider = services.BuildServiceProvider();
            using IServiceScope scope = provider.CreateScope();

            IRepositoryProxyPersonWithCustomMethod repository =
                scope.ServiceProvider.GetService<IRepositoryProxyPersonWithCustomMethod>();

            Assert.IsNull(repository);
        }
    }
}
