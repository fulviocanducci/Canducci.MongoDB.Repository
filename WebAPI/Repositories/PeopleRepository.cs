using Canducci.MongoDB.Repository;
using MongoDB.Driver;

namespace WebAPI.Repositories
{
   public sealed class PeopleRepository : PeopleRepositoryAbstract
   {
      public PeopleRepository(IConnect connect) : base(connect)
      {
      }

      public PeopleRepository(IConnect connect, MongoCollectionSettings mongoCollectionSettings) : base(connect, mongoCollectionSettings)
      {
      }
   }
}
