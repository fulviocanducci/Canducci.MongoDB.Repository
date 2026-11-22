using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryFindBy<T> where T : class, new()
    {
        #region FindBy
        IReadOnlyList<T> FindBy(
            FilterDefinition<T> filter,
            SortDefinition<T> sort,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<T>> FindByAsync(
            FilterDefinition<T> filter,
            SortDefinition<T> sort,
            CancellationToken cancellationToken = default);
        #endregion
    }
}