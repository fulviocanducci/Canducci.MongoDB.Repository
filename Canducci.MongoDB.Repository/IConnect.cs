using MongoDB.Driver;

namespace Canducci.MongoDB.Repository
{
    public interface IConnect
    {
        IMongoCollection<T> Collection<T>(string collectionName);
        IMongoCollection<T> Collection<T>(string collectionName, MongoCollectionSettings mongoCollectionSettings);
    }
}
