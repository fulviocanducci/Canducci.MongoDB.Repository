using Canducci.MongoDB.Repository.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class UnitTestBsonCollectionName
    {
        [TestMethod]
        public void TestInstanceBsonCollectionName()
        {
            BsonCollectionName collectionName = new BsonCollectionName("person");
            Assert.IsInstanceOfType(collectionName, typeof(BsonCollectionName));
            Assert.AreEqual("person", collectionName.Name);
            Assert.IsNotNull(collectionName.Name);
        }
    }
}
