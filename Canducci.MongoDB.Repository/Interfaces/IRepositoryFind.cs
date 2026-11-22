using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
    public interface IRepositoryFind<T> where T : class, new()
    {
        #region Find
        T Find<TKey>(TKey key, CancellationToken cancellationToken = default);
        T Find(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        Task<T> FindAsync<TKey>(TKey key, CancellationToken cancellationToken = default);
        Task<T> FindAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        #endregion
    }
}