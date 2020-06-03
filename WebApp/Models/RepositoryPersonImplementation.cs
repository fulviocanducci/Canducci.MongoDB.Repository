using Canducci.MongoDB.Repository;
namespace WebApp.Models
{
    public abstract class RepositoryPersonImplementation : Repository<Person>
    {
        protected RepositoryPersonImplementation(IConnect connect) : base(connect)
        {
        }
    }
}
