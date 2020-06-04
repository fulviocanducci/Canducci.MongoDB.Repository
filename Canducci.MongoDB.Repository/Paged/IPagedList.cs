using MongoDB.Driver.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Canducci.MongoDB.Repository.Paged
{
    public interface IPagedList<T> : IPagedList, IEnumerable<T>
    {
        T this[int index] { get; }
        int Count { get; }
        IPagedList GetMetaData();
    }

    public interface IPagedList
    {
        int PageCount { get; }
        int TotalItemCount { get; }
        int PageNumber { get; }
        int PageSize { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        bool IsFirstPage { get; }
        bool IsLastPage { get; }
        int FirstItemOnPage { get; }
        int LastItemOnPage { get; }
    }

}
