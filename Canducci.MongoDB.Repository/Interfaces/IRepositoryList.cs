using System.Collections.Generic;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryList<T> where T : class, new()
    {
        #region List
        IReadOnlyList<T> List<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null);
        Task<IReadOnlyList<T>> ListAsync<Tkey>(Expression<Func<T, Tkey>> orderBy, Expression<Func<T, bool>> filter = null, CancellationToken cancellationToken = default);
        #endregion
    }
}