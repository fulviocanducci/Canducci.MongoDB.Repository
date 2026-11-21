using Canducci.MongoDB.Repository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class RepositoryBsonIdTests
    {
        [TestMethod]
        public void Constructor_ShouldDetectIdPropertyByConvention()
        {
            TestRepository<ModelWithId> repository = new TestRepository<ModelWithId>();

            Assert.AreEqual(nameof(ModelWithId.Id), repository.BsonIdMemberName);
        }

        [TestMethod]
        public void Constructor_ShouldDetectLowercaseIdPropertyByConvention()
        {
            TestRepository<ModelWithLowercaseId> repository = new TestRepository<ModelWithLowercaseId>();

            Assert.AreEqual(nameof(ModelWithLowercaseId.id), repository.BsonIdMemberName);
        }

        [TestMethod]
        public void Constructor_ShouldDetectUnderscoreIdPropertyByConvention()
        {
            TestRepository<ModelWithUnderscoreId> repository = new TestRepository<ModelWithUnderscoreId>();

            Assert.AreEqual(nameof(ModelWithUnderscoreId._id), repository.BsonIdMemberName);
        }

        [TestMethod]
        public void Constructor_ShouldDetectPropertyDecoratedWithBsonIdAttribute()
        {
            TestRepository<ModelWithBsonIdAttribute> repository = new TestRepository<ModelWithBsonIdAttribute>();

            Assert.AreEqual(nameof(ModelWithBsonIdAttribute.Key), repository.BsonIdMemberName);
        }

        [TestMethod]
        public void Constructor_ShouldNotThrowWhenModelDoesNotHaveIdMember()
        {
            TestRepository<ModelWithoutId> repository = new TestRepository<ModelWithoutId>();

            Assert.IsNull(repository.BsonIdMemberName);
        }

        private sealed class TestRepository<TEntity> : Repository<TEntity> where TEntity : class, new()
        {
            public TestRepository() : base(new StubConnect())
            {
            }

            public string BsonIdMemberName => BsonIdName;
        }

        private sealed class StubConnect : IConnect
        {
            public IMongoCollection<T> Collection<T>(string collectionName)
            {
                return null;
            }

            public IMongoCollection<T> Collection<T>(string collectionName, MongoCollectionSettings mongoCollectionSettings)
            {
                return null;
            }
        }

        public sealed class ModelWithId
        {
            public string Id { get; set; }
        }

        public sealed class ModelWithLowercaseId
        {
            public string id { get; set; }
        }

        public sealed class ModelWithUnderscoreId
        {
            public string _id { get; set; }
        }

        public sealed class ModelWithBsonIdAttribute
        {
            [BsonId]
            public string Key { get; set; }
        }

        public sealed class ModelWithoutId
        {
            public string Name { get; set; }
        }
    }
}
