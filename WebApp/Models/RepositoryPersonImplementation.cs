using Canducci.MongoDB.Repository;
using MongoDB.Driver;

namespace WebApp.Models
{
    public abstract class RepositoryPersonImplementation : Repository<Person>
    {
        public RepositoryPersonImplementation(IConnect connect)
            : base(connect)
        { }

        //public RepositoryPersonImplementation(IConnect connect, MongoCollectionSettings mongoCollectionSettings)
        //    : base(connect, mongoCollectionSettings)
        //{ }
    }
}
