using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class RepositoryContractTests
    {
        private MongoDbRunner _runner;
        private IMongoCollection<TestDocument> _collection;
        private TestRepository _repository;

        [TestInitialize]
        public void Initialize()
        {
            _runner = MongoDbRunner.Start();
            MongoClient client = new MongoClient(_runner.ConnectionString);
            IMongoDatabase database = client.GetDatabase("RepositoryContractTests");
            _collection = database.GetCollection<TestDocument>(Guid.NewGuid().ToString("N"));
            _repository = new TestRepository(_collection);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _runner?.Dispose();
        }

        [TestMethod]
        public void Edit_WhenDocumentMatchesButReplacementIsUnchanged_ReturnsTrue()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Fulvio" });

            bool result = _repository.Edit(document.Id, new TestDocument { Id = document.Id, Name = document.Name });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task EditAsync_WhenDocumentMatchesButReplacementIsUnchanged_ReturnsTrue()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Fulvio" });

            bool result = await _repository.EditAsync(x => x.Id == document.Id, new TestDocument { Id = document.Id, Name = document.Name });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Update_ReturnsMongoUpdateResult()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Before" });

            UpdateResult result = _repository.Update(
                x => x.Id == document.Id,
                Builders<TestDocument>.Update.Set(x => x.Name, "After"));

            Assert.IsTrue(result.IsAcknowledged);
            Assert.AreEqual(1L, result.MatchedCount);
            Assert.AreEqual(1L, result.ModifiedCount);
        }

        [TestMethod]
        public async Task UpdateAsync_ReturnsMongoUpdateResult()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Before" });

            UpdateResult result = await _repository.UpdateAsync(
                x => x.Id == document.Id,
                Builders<TestDocument>.Update.Set(x => x.Name, "After"));

            Assert.IsTrue(result.IsAcknowledged);
            Assert.AreEqual(1L, result.MatchedCount);
            Assert.AreEqual(1L, result.ModifiedCount);
        }

        [TestMethod]
        public void UpdateAll_ReturnsCountsForAllMatchingDocuments()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "Before" },
                new TestDocument { Name = "Before" }
            });

            UpdateResult result = _repository.UpdateAll(
                x => x.Name == "Before",
                Builders<TestDocument>.Update.Set(x => x.Name, "After"));

            Assert.AreEqual(2L, result.MatchedCount);
            Assert.AreEqual(2L, result.ModifiedCount);
        }

        [TestMethod]
        public async Task UpdateAllAsync_ReturnsCountsForAllMatchingDocuments()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "Before" },
                new TestDocument { Name = "Before" }
            });

            UpdateResult result = await _repository.UpdateAllAsync(
                x => x.Name == "Before",
                Builders<TestDocument>.Update.Set(x => x.Name, "After"));

            Assert.AreEqual(2L, result.MatchedCount);
            Assert.AreEqual(2L, result.ModifiedCount);
        }

        [TestMethod]
        public void CollectionMethods_ReturnMaterializedReadOnlySnapshots()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "B" },
                new TestDocument { Name = "A" }
            });

            IReadOnlyList<TestDocument> all = _repository.All();
            IReadOnlyList<TestDocument> filtered = _repository.All(x => x.Name == "A");
            IReadOnlyList<TestDocument> ordered = _repository.List(x => x.Name);

            _repository.Add(new TestDocument { Name = "C" });

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("A", ordered[0].Name);
        }

        [TestMethod]
        public async Task AsyncCollectionMethods_AcceptCancellationTokenAndReturnMaterializedLists()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "B" },
                new TestDocument { Name = "A" }
            });
            CancellationToken cancellationToken = CancellationToken.None;

            IReadOnlyList<TestDocument> all = await _repository.AllAsync(cancellationToken);
            IReadOnlyList<TestDocument> filtered = await _repository.AllAsync(x => x.Name == "A", cancellationToken);
            IReadOnlyList<TestDocument> ordered = await _repository.AllAsync(x => true, x => x.Name, cancellationToken);
            IReadOnlyList<TestDocument> listed = await _repository.ListAsync(x => x.Name, null, cancellationToken);

            Assert.AreEqual(2, all.Count);
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("A", ordered[0].Name);
            Assert.AreEqual("A", listed[0].Name);
        }

        [TestMethod]
        public void AddMany_MaterializesInputOnlyOnceAndReturnsStableList()
        {
            int enumerations = 0;

            IEnumerable<TestDocument> GetDocuments()
            {
                enumerations++;
                yield return new TestDocument { Name = "One" };
                yield return new TestDocument { Name = "Two" };
            }

            IReadOnlyList<TestDocument> inserted = _repository.Add(GetDocuments());

            Assert.AreEqual(1, enumerations);
            Assert.AreEqual(2, inserted.Count);
            Assert.AreEqual(2, _repository.Count());
        }

        [TestMethod]
        public void Edit_WithFilter_WhenDocumentMatchesButReplacementIsUnchanged_ReturnsTrue()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Fulvio" });

            bool result = _repository.Edit(x => x.Id == document.Id, new TestDocument { Id = document.Id, Name = document.Name });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task EditAsync_WithKey_WhenDocumentMatchesButReplacementIsUnchanged_ReturnsTrue()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Fulvio" });

            bool result = await _repository.EditAsync(document.Id, new TestDocument { Id = document.Id, Name = document.Name });

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Edit_WhenNoDocumentMatches_ReturnsFalse()
        {
            bool result = _repository.Edit(x => x.Name == "Missing", new TestDocument { Name = "Replacement" });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Update_WhenDocumentMatchesButValueIsUnchanged_PreservesMatchedAndModifiedCounts()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Same" });

            UpdateResult result = _repository.Update(
                x => x.Id == document.Id,
                Builders<TestDocument>.Update.Set(x => x.Name, "Same"));

            Assert.AreEqual(1L, result.MatchedCount);
            Assert.AreEqual(0L, result.ModifiedCount);
        }

        [TestMethod]
        public async Task AddManyAsync_MaterializesInputAndReturnsReadOnlyList()
        {
            IReadOnlyList<TestDocument> inserted = await _repository.AddAsync(new[]
            {
                new TestDocument { Name = "One" },
                new TestDocument { Name = "Two" }
            });

            Assert.AreEqual(2, inserted.Count);
            Assert.AreEqual(2, _repository.Count());
        }

        private sealed class TestRepository : Repository<TestDocument>
        {
            public TestRepository(IMongoCollection<TestDocument> collection)
                : base(new StubConnect(collection))
            {
            }
        }

        private sealed class StubConnect : IConnect
        {
            private readonly IMongoCollection<TestDocument> _collection;

            public StubConnect(IMongoCollection<TestDocument> collection)
            {
                _collection = collection;
            }

            public IMongoCollection<T> Collection<T>(string collectionName)
            {
                return (IMongoCollection<T>)(object)_collection;
            }

            public IMongoCollection<T> Collection<T>(string collectionName, MongoCollectionSettings mongoCollectionSettings)
            {
                return (IMongoCollection<T>)(object)_collection;
            }
        }

        public sealed class TestDocument
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }
    }
}