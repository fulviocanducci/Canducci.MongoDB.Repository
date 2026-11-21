using MongoDB.Driver;

namespace Canducci.MongoDB.Repository
{
    internal sealed class RepositoryProxy<T> : Repository<T>
        where T : class, new()
    {
        public RepositoryProxy(IConnect connect)
            : base(connect)
        {
        }

        public RepositoryProxy(IConnect connect, MongoCollectionSettings mongoCollectionSettings)
            : base(connect, mongoCollectionSettings)
        {
        }
    }
}
