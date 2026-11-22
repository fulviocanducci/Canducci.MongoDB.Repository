using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryAdd<T> where T : class, new()
    {
        #region Add
        T Add(T model, CancellationToken cancellationToken = default);
        IReadOnlyList<T> Add(IEnumerable<T> models, CancellationToken cancellationToken = default);
        Task<T> AddAsync(T model, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> AddAsync(IEnumerable<T> models, CancellationToken cancellationToken = default);
        #endregion
    }
}