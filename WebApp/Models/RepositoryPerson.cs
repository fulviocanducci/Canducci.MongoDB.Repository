using Canducci.MongoDB.Repository;

namespace WebApp.Models
{
    public sealed class RepositoryPerson : RepositoryPersonImplementation
    {
        public RepositoryPerson(IConnect connect) : base(connect)
        {
        }
    }
}
