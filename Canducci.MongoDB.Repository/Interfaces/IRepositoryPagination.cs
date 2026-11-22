using Canducci.Pagination.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryPagination<T> where T : class, new()
    {
        #region Pagination
        IPaginated<T> PagedList(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<IPaginated<T>> PagedListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        IPaginated<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default);
        Task<IPaginated<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default);
        IPaginated<T> PagedList<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default);
        Task<IPaginated<T>> PagedListAsync<Tkey>(int pageNumber, int pageSize, Expression<Func<T, bool>> filter, Expression<Func<T, Tkey>> orderBy, CancellationToken cancellationToken = default);
        #endregion
    }
}