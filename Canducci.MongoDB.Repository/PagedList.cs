using Canducci.Pagination.Bases;
using Canducci.Pagination.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Canducci.MongoDB.Repository
{
    public sealed class PagedList<T> : PaginatedBase<T>, IPaginated<T>
    {
        public PagedList(IEnumerable<T> subset, int pageNumber, int pageSize, int totalItemCount)
            : base(subset ?? Enumerable.Empty<T>())
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber cannot be below 1.");
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
            }
            if (totalItemCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalItemCount), totalItemCount, "TotalItemCount cannot be negative.");
            }

            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = totalItemCount > 0
                ? (int)Math.Ceiling(totalItemCount / (double)pageSize)
                : 0;
            HasPreviousPage = pageNumber > 1;
            HasNextPage = pageNumber < PageCount;
            IsFirstPage = pageNumber == 1;
            IsLastPage = pageNumber >= PageCount;
            FirstItemOnPage = (pageNumber - 1) * pageSize + 1;
            int lastItemOnPage = FirstItemOnPage + pageSize - 1;
            LastItemOnPage = lastItemOnPage > totalItemCount
                ? totalItemCount
                : lastItemOnPage;

            SetPages();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}