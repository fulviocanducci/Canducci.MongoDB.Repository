using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryCount<T> where T : class, new()
    {
        #region Count
        long Count(CancellationToken cancellationToken = default);
        long Count(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default);
        Task<long> CountAsync(CancellationToken cancellationToken = default);
        Task<long> CountAsync(Expression<Func<T, bool>> filter, CountOptions options = null, CancellationToken cancellationToken = default);
        #endregion
    }
}