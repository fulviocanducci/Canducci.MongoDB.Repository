using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
    public interface IRepositoryDelete<T> where T : class, new()
    {
        #region Delete
        bool Delete(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        bool Delete<TKey>(TKey key, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync<TKey>(TKey key, CancellationToken cancellationToken = default);
        #endregion
    }
}