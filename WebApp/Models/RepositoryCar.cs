using Canducci.MongoDB.Repository;

namespace WebApp.Models
{
    public sealed class RepositoryCar : RepositoryCarImplementation
    {
        public RepositoryCar(IConnect connect)
            : base(connect)
        { }

        //public RepositoryPerson(IConnect connect, MongoCollectionSettings mongoCollectionSettings) 
        //    : base(connect, mongoCollectionSettings)
        //{ }
    }
}
