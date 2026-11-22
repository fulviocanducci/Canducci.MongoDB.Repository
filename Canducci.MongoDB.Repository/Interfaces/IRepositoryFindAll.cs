using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryFindAll<T> where T : class, new()
    {
        #region FindAll
        IReadOnlyList<T> FindAll(
            SortDefinition<T> sort,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> FindAllAsync(
            SortDefinition<T> sort,
            CancellationToken cancellationToken = default);
        #endregion
    }
}