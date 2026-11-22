using MongoDB.Driver;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryUpdate<T> where T : class, new()
    {
        #region Update
        UpdateResult Update(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        UpdateResult UpdateAll(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        Task<UpdateResult> UpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        Task<UpdateResult> UpdateAllAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, CancellationToken cancellationToken = default);
        #endregion
    }
}