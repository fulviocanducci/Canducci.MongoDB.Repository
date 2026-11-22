using System.Linq;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryQueryable<T> where T : class, new()
    {
        #region IQueryable
        IQueryable<T> Query();
        #endregion
    }
}