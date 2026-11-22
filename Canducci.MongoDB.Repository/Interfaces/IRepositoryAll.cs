using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository
{
    public interface IRepositoryAll<T> where T : class, new()
    {
        #region All
        IReadOnlyList<T> All();
        IReadOnlyList<T> All(Expression<Func<T, bool>> filter);
        IReadOnlyList<T> All<TKey>(Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderBy);
        Task<IReadOnlyList<T>> AllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> AllAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> AllAsync<TKey>(Expression<Func<T, bool>> filter, Expression<Func<T, TKey>> orderBy, CancellationToken cancellationToken = default);
        #endregion
    }
}