using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Canducci.MongoDB.Repository.Attributes;
using Canducci.Pagination.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class RepositoryOperationsTests
    {
        private static MongoDbRunner _runner;
        private IMongoDatabase _database;
        private TestRepository _repository;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _runner = MongoDbRunner.Start();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _runner?.Dispose();
        }

        [TestInitialize]
        public void Initialize()
        {
            MongoClient client = new MongoClient(_runner.ConnectionString);
            _database = client.GetDatabase("RepositoryOperationsTests_" + Guid.NewGuid().ToString("N"));
            _repository = new TestRepository(new DatabaseConnect(_database));
        }

        [TestMethod]
        public void Add_ShouldInsertAndReturnDocument()
        {
            TestDocument document = new TestDocument { Name = "Fulvio", Category = "A" };

            TestDocument result = _repository.Add(document);

            Assert.AreSame(document, result);
            Assert.AreEqual(document.Id, _repository.Find(document.Id).Id);
        }

        [TestMethod]
        public async Task AddAsync_ShouldInsertAndReturnDocument()
        {
            TestDocument document = new TestDocument { Name = "Fulvio", Category = "A" };

            TestDocument result = await _repository.AddAsync(document);

            Assert.AreSame(document, result);
            Assert.AreEqual(document.Id, (await _repository.FindAsync(document.Id)).Id);
        }

        [TestMethod]
        public void Find_ShouldSupportKeyAndFilter()
        {
            TestDocument document = _repository.Add(new TestDocument { Name = "Fulvio", Category = "A" });

            TestDocument byKey = _repository.Find(document.Id);
            TestDocument byFilter = _repository.Find(x => x.Name == "Fulvio");

            Assert.AreEqual(document.Id, byKey.Id);
            Assert.AreEqual(document.Id, byFilter.Id);
        }

        [TestMethod]
        public async Task FindAsync_ShouldSupportKeyAndFilter()
        {
            TestDocument document = await _repository.AddAsync(new TestDocument { Name = "Fulvio", Category = "A" });

            TestDocument byKey = await _repository.FindAsync(document.Id);
            TestDocument byFilter = await _repository.FindAsync(x => x.Name == "Fulvio");

            Assert.AreEqual(document.Id, byKey.Id);
            Assert.AreEqual(document.Id, byFilter.Id);
        }

        [TestMethod]
        public void Find_WhenDocumentDoesNotExist_ReturnsNull()
        {
            Assert.IsNull(_repository.Find(ObjectId.GenerateNewId()));
            Assert.IsNull(_repository.Find(x => x.Name == "Missing"));
        }

        [TestMethod]
        public async Task FindAsync_WhenDocumentDoesNotExist_ReturnsNull()
        {
            Assert.IsNull(await _repository.FindAsync(ObjectId.GenerateNewId()));
            Assert.IsNull(await _repository.FindAsync(x => x.Name == "Missing"));
        }

        [TestMethod]
        public void Delete_ShouldSupportKeyAndFilter()
        {
            TestDocument byKey = _repository.Add(new TestDocument { Name = "ByKey" });
            TestDocument byFilter = _repository.Add(new TestDocument { Name = "ByFilter" });

            Assert.IsTrue(_repository.Delete(byKey.Id));
            Assert.IsTrue(_repository.Delete(x => x.Id == byFilter.Id));
            Assert.IsFalse(_repository.Delete(byKey.Id));
            Assert.IsFalse(_repository.Delete(x => x.Id == byFilter.Id));
        }

        [TestMethod]
        public async Task DeleteAsync_ShouldSupportKeyAndFilter()
        {
            TestDocument byKey = await _repository.AddAsync(new TestDocument { Name = "ByKey" });
            TestDocument byFilter = await _repository.AddAsync(new TestDocument { Name = "ByFilter" });

            Assert.IsTrue(await _repository.DeleteAsync(byKey.Id));
            Assert.IsTrue(await _repository.DeleteAsync(x => x.Id == byFilter.Id));
            Assert.IsFalse(await _repository.DeleteAsync(byKey.Id));
            Assert.IsFalse(await _repository.DeleteAsync(x => x.Id == byFilter.Id));
        }

        [TestMethod]
        public void Count_ShouldReturnTotalAndFilteredCounts()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "One", Category = "A" },
                new TestDocument { Name = "Two", Category = "A" },
                new TestDocument { Name = "Three", Category = "B" }
            });

            Assert.AreEqual(3L, _repository.Count());
            Assert.AreEqual(2L, _repository.Count(x => x.Category == "A"));
            Assert.AreEqual(0L, _repository.Count(x => x.Category == "Missing"));
        }

        [TestMethod]
        public async Task CountAsync_ShouldReturnTotalAndFilteredCounts()
        {
            await _repository.AddAsync(new[]
            {
                new TestDocument { Name = "One", Category = "A" },
                new TestDocument { Name = "Two", Category = "A" },
                new TestDocument { Name = "Three", Category = "B" }
            });

            Assert.AreEqual(3L, await _repository.CountAsync());
            Assert.AreEqual(2L, await _repository.CountAsync(x => x.Category == "A"));
            Assert.AreEqual(0L, await _repository.CountAsync(x => x.Category == "Missing"));
        }

        [TestMethod]
        public void AllAndList_ShouldApplyFiltersAndAscendingSort()
        {
            SeedDocuments();

            IReadOnlyList<TestDocument> all = _repository.All(x => x.Category == "A", x => x.Name);
            IReadOnlyList<TestDocument> list = _repository.List(x => x.Name, x => x.Category == "B");

            Assert.AreEqual(3, all.Count);
            Assert.AreEqual("Alpha", all[0].Name);
            Assert.AreEqual("Charlie", all[1].Name);
            Assert.AreEqual("Echo", all[2].Name);
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Bravo", list[0].Name);
            Assert.AreEqual("Delta", list[1].Name);
        }

        [TestMethod]
        public async Task ListAsync_ShouldApplyFilterAndAscendingSort()
        {
            SeedDocuments();

            IReadOnlyList<TestDocument> list = await _repository.ListAsync(x => x.Name, x => x.Category == "B");

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Bravo", list[0].Name);
            Assert.AreEqual("Delta", list[1].Name);
        }

        [TestMethod]
        public void FindBy_ShouldApplyMongoFilterAndSort()
        {
            SeedDocuments();

            FilterDefinition<TestDocument> filter = Builders<TestDocument>.Filter.Eq(x => x.Category, "A");
            SortDefinition<TestDocument> sort = Builders<TestDocument>.Sort.Ascending(x => x.Name);

            IReadOnlyList<TestDocument> result = _repository.FindBy(filter, sort);

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Alpha", result[0].Name);
            Assert.AreEqual("Charlie", result[1].Name);
            Assert.AreEqual("Echo", result[2].Name);
        }

        [TestMethod]
        public async Task FindByAsync_ShouldApplyMongoFilterAndSort()
        {
            SeedDocuments();

            FilterDefinition<TestDocument> filter = Builders<TestDocument>.Filter.Eq(x => x.Category, "B");
            SortDefinition<TestDocument> sort = Builders<TestDocument>.Sort.Descending(x => x.Name);

            IReadOnlyList<TestDocument> result = await _repository.FindByAsync(filter, sort);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Delta", result[0].Name);
            Assert.AreEqual("Bravo", result[1].Name);
        }

        [TestMethod]
        public void FindAll_ShouldReturnAllDocumentsInMongoSortOrder()
        {
            SeedDocuments();

            SortDefinition<TestDocument> sort = Builders<TestDocument>.Sort.Descending(x => x.Name);

            IReadOnlyList<TestDocument> result = _repository.FindAll(sort);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("Echo", result[0].Name);
            Assert.AreEqual("Delta", result[1].Name);
            Assert.AreEqual("Charlie", result[2].Name);
            Assert.AreEqual("Bravo", result[3].Name);
            Assert.AreEqual("Alpha", result[4].Name);
        }

        [TestMethod]
        public async Task FindAllAsync_ShouldReturnAllDocumentsInMongoSortOrder()
        {
            SeedDocuments();

            SortDefinition<TestDocument> sort = Builders<TestDocument>.Sort.Ascending(x => x.Name);

            IReadOnlyList<TestDocument> result = await _repository.FindAllAsync(sort);

            Assert.AreEqual(5, result.Count);
            Assert.AreEqual("Alpha", result[0].Name);
            Assert.AreEqual("Bravo", result[1].Name);
            Assert.AreEqual("Charlie", result[2].Name);
            Assert.AreEqual("Delta", result[3].Name);
            Assert.AreEqual("Echo", result[4].Name);
        }

        [TestMethod]
        public void Update_WhenNoDocumentsMatch_ReturnsZeroCounts()
        {
            UpdateResult result = _repository.Update(
                x => x.Name == "Missing",
                Builders<TestDocument>.Update.Set(x => x.Name, "Updated"));

            Assert.AreEqual(0L, result.MatchedCount);
            Assert.AreEqual(0L, result.ModifiedCount);
        }

        [TestMethod]
        public async Task UpdateAsync_WhenNoDocumentsMatch_ReturnsZeroCounts()
        {
            UpdateResult result = await _repository.UpdateAsync(
                x => x.Name == "Missing",
                Builders<TestDocument>.Update.Set(x => x.Name, "Updated"));

            Assert.AreEqual(0L, result.MatchedCount);
            Assert.AreEqual(0L, result.ModifiedCount);
        }

        [TestMethod]
        public async Task AddManyAsync_ShouldEnumerateSingleUseInputOnlyOnce()
        {
            int enumerations = 0;

            IEnumerable<TestDocument> GetDocuments()
            {
                enumerations++;
                yield return new TestDocument { Name = "One" };
                yield return new TestDocument { Name = "Two" };
            }

            IReadOnlyList<TestDocument> inserted = await _repository.AddAsync(GetDocuments());

            Assert.AreEqual(1, enumerations);
            Assert.AreEqual(2, inserted.Count);
            Assert.AreEqual(2L, await _repository.CountAsync());
        }

        [TestMethod]
        public async Task AllAsync_ShouldHonorCanceledCancellationToken()
        {
            using (CancellationTokenSource cancellation = new CancellationTokenSource())
            {
                cancellation.Cancel();
                bool wasCanceled = false;

                try
                {
                    await _repository.AllAsync(cancellation.Token);
                }
                catch (OperationCanceledException)
                {
                    wasCanceled = true;
                }

                Assert.IsTrue(wasCanceled);
            }
        }

        [TestMethod]
        public void Query_ShouldExposeQueryableCollection()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "One", Category = "A" },
                new TestDocument { Name = "Two", Category = "B" }
            });

            List<string> names = _repository.Query()
                .Where(x => x.Category == "A")
                .Select(x => x.Name)
                .ToList();

            CollectionAssert.AreEqual(new[] { "One" }, names);
        }

        [TestMethod]
        public void PagedList_ShouldSupportUnfilteredOrderedAndFilteredQueries()
        {
            SeedDocuments();

            IPaginated<TestDocument> unfiltered = _repository.PagedList(2, 2);
            IPaginated<TestDocument> ordered = _repository.PagedList(2, 2, x => x.Name);
            IPaginated<TestDocument> filtered = _repository.PagedList(1, 2, x => x.Category == "A", x => x.Name);

            Assert.AreEqual(5, unfiltered.TotalItemCount);
            Assert.AreEqual(2, ((ICollection<TestDocument>)unfiltered).Count);
            Assert.AreEqual(5, ordered.TotalItemCount);
            Assert.AreEqual("Charlie", ordered.First().Name);
            Assert.AreEqual(3, filtered.TotalItemCount);
            Assert.AreEqual("Alpha", filtered.First().Name);
        }

        [TestMethod]
        public async Task PagedListAsync_ShouldSupportUnfilteredOrderedAndFilteredQueries()
        {
            SeedDocuments();

            IPaginated<TestDocument> unfiltered = await _repository.PagedListAsync(2, 2);
            IPaginated<TestDocument> ordered = await _repository.PagedListAsync(2, 2, x => x.Name);
            IPaginated<TestDocument> filtered = await _repository.PagedListAsync(1, 2, x => x.Category == "A", x => x.Name);

            Assert.AreEqual(5, unfiltered.TotalItemCount);
            Assert.AreEqual(2, ((ICollection<TestDocument>)unfiltered).Count);
            Assert.AreEqual(5, ordered.TotalItemCount);
            Assert.AreEqual("Charlie", ordered.First().Name);
            Assert.AreEqual(3, filtered.TotalItemCount);
            Assert.AreEqual("Alpha", filtered.First().Name);
        }

        [TestMethod]
        public void PagedList_ShouldRejectInvalidPageArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _repository.PagedList(0, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => _repository.PagedList(1, 0));
        }

        [TestMethod]
        public async Task PagedListAsync_ShouldRejectInvalidPageArguments()
        {
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.PagedListAsync(0, 2));
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _repository.PagedListAsync(1, 0));
        }

        [TestMethod]
        public void CreateObjectId_ShouldParseValidIdentifier()
        {
            string value = ObjectId.GenerateNewId().ToString();

            ObjectId result = _repository.CreateObjectId(value);

            Assert.AreEqual(value, result.ToString());
        }

        [TestMethod]
        public void CreateObjectId_ShouldRejectInvalidIdentifier()
        {
            Assert.Throws<FormatException>(() => _repository.CreateObjectId("invalid"));
        }

        [TestMethod]
        public void Repository_ShouldResolveBsonIdMemberNameForStringKeys()
        {
            StringIdRepository repository = new StringIdRepository(new DatabaseConnect(_database));
            StringIdDocument document = repository.Add(new StringIdDocument { Key = "person-001", Name = "Fulvio" });

            StringIdDocument result = repository.Find("person-001");

            Assert.AreEqual(document.Key, result.Key);
            Assert.IsTrue(repository.Delete("person-001"));
        }

        [TestMethod]
        public void Repository_ShouldUseCollectionNameAttribute()
        {
            DatabaseConnect connect = new DatabaseConnect(_database);
            CustomCollectionRepository repository = new CustomCollectionRepository(connect);

            Assert.AreEqual("people_custom", connect.LastCollectionName);
            Assert.IsNotNull(repository.Collection);
        }

        [TestMethod]
        public void Repository_ShouldPassCollectionSettingsToConnection()
        {
            DatabaseConnect connect = new DatabaseConnect(_database);
            MongoCollectionSettings settings = new MongoCollectionSettings();

            TestRepository repository = new TestRepository(connect, settings);

            Assert.AreSame(settings, connect.LastSettings);
            Assert.IsNotNull(repository.Collection);
        }

        private void SeedDocuments()
        {
            _repository.Add(new[]
            {
                new TestDocument { Name = "Echo", Category = "A" },
                new TestDocument { Name = "Alpha", Category = "A" },
                new TestDocument { Name = "Delta", Category = "B" },
                new TestDocument { Name = "Charlie", Category = "A" },
                new TestDocument { Name = "Bravo", Category = "B" }
            });
        }

        private sealed class TestRepository : Repository<TestDocument>
        {
            public TestRepository(IConnect connect)
                : base(connect)
            {
            }

            public TestRepository(IConnect connect, MongoCollectionSettings settings)
                : base(connect, settings)
            {
            }
        }

        private sealed class StringIdRepository : Repository<StringIdDocument>
        {
            public StringIdRepository(IConnect connect)
                : base(connect)
            {
            }
        }

        private sealed class CustomCollectionRepository : Repository<CustomCollectionDocument>
        {
            public CustomCollectionRepository(IConnect connect)
                : base(connect)
            {
            }
        }

        private sealed class DatabaseConnect : IConnect
        {
            private readonly IMongoDatabase _database;

            public DatabaseConnect(IMongoDatabase database)
            {
                _database = database;
            }

            public string LastCollectionName { get; private set; }
            public MongoCollectionSettings LastSettings { get; private set; }

            public IMongoCollection<T> Collection<T>(string collectionName)
            {
                LastCollectionName = collectionName;
                LastSettings = null;
                return _database.GetCollection<T>(collectionName);
            }

            public IMongoCollection<T> Collection<T>(string collectionName, MongoCollectionSettings mongoCollectionSettings)
            {
                LastCollectionName = collectionName;
                LastSettings = mongoCollectionSettings;
                return _database.GetCollection<T>(collectionName, mongoCollectionSettings);
            }
        }

        public sealed class TestDocument
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
            public string Category { get; set; }
        }

        public sealed class StringIdDocument
        {
            [BsonId]
            public string Key { get; set; }
            public string Name { get; set; }
        }

        [BsonCollectionName("people_custom")]
        public sealed class CustomCollectionDocument
        {
            public ObjectId Id { get; set; }
            public string Name { get; set; }
        }
    }
}
