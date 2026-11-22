using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
    public interface IRepositoryEdit<T> where T : class, new()
    {
        #region Edit
        bool Edit(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default);
        Task<bool> EditAsync(Expression<Func<T, bool>> filter, T model, CancellationToken cancellationToken = default);
        bool Edit<TKey>(TKey key, T model, CancellationToken cancellationToken = default);
        Task<bool> EditAsync<TKey>(TKey key, T model, CancellationToken cancellationToken = default);
        #endregion
    }
}