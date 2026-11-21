using Canducci.Pagination.Interfaces;

namespace Canducci.MongoDB.Repository.Paged
{
    public interface IPagedList<T> : IPagedList, IPaginated<T>
    {
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
