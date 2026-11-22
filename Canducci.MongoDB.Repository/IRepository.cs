using Canducci.MongoDB.Repository.Interfaces;

namespace Canducci.MongoDB.Repository
{
    public interface IRepository<T> :
        IRepositoryMongoCollection<T>,
        IRepositoryAdd<T>,
        IRepositoryEdit<T>,
        IRepositoryUpdate<T>,
        IRepositoryFind<T>,
        IRepositoryAll<T>,
        IRepositoryFindBy<T>,
        IRepositoryFindAll<T>,
        IRepositoryList<T>,
        IRepositoryDelete<T>,
        IRepositoryQueryable<T>,
        IRepositoryPagination<T>,
        IRepositoryCreateObjectId<T>,
        IRepositoryCount<T>
        where T : class, new()
    {
    }
}