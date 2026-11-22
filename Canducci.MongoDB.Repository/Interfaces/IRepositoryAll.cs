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
        IReadOnlyList<T> All(Expression<System.Func<T, bool>> filter);
        IReadOnlyList<T> All<Tkey>(Expression<System.Func<T, bool>> filter, Expression<System.Func<T, Tkey>> orderBy);
        Task<IReadOnlyList<T>> AllAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> AllAsync(Expression<System.Func<T, bool>> filter, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> AllAsync<Tkey>(Expression<System.Func<T, bool>> filter, Expression<System.Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default);
        #endregion
    }
}