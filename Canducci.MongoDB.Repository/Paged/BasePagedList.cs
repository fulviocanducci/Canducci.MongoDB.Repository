using System;
using System.Collections;
using System.Collections.Generic;

namespace Canducci.MongoDB.Repository.Paged
{
    [Serializable]
    public abstract class BasePagedList<T> : PagedListMetaData, IPagedList<T>
    {
        protected readonly List<T> Subset = new List<T>();
        protected internal BasePagedList() { }
        protected internal BasePagedList(int pageNumber, int pageSize, int totalItemCount)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            }
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");
            }

            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount ? TotalItemCount : numberOfLastItemOnPage;
        }

        #region IPagedList<T> Members


        public IEnumerator<T> GetEnumerator()
        {
            return Subset.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        public T this[int index]
        {
            get { return Subset[index]; }
        }
        public int Count
        {
            get { return Subset.Count; }
        }
        public IPagedList GetMetaData()
        {
            return new PagedListMetaData(this);
        }

        #endregion
    }

}
