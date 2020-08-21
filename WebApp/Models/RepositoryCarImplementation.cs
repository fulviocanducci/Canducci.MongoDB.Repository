using Canducci.MongoDB.Repository;

namespace WebApp.Models
{
    public abstract class RepositoryCarImplementation : Repository<Car>
    {
        public RepositoryCarImplementation(IConnect connect)
            : base(connect)
        { }

        //public RepositoryCarImplementation(IConnect connect, MongoCollectionSettings mongoCollectionSettings)
        //    : base(connect, mongoCollectionSettings)
        //{ }
    }
}
