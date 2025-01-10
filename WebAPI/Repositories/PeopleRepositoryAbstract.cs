using Canducci.MongoDB.Repository;
using MongoDB.Driver;
using WebAPI.Models;

namespace WebAPI.Repositories
{
   public class PeopleRepositoryAbstract : Repository<People>
   {
      public PeopleRepositoryAbstract(IConnect connect) : base(connect)
      {
      }

      public PeopleRepositoryAbstract(IConnect connect, MongoCollectionSettings mongoCollectionSettings) : base(connect, mongoCollectionSettings)
      {
      }
   }
}
