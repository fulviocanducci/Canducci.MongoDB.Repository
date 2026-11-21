using Canducci.Pagination.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Canducci.MongoDB.Repository.Paged
{
    [Serializable]
    public abstract class BasePagedList<T> : PagedListMetaData, IPagedList<T>
    {
        protected readonly List<T> Subset = new List<T>();

        protected internal BasePagedList()
        {
        }

        protected internal BasePagedList(int pageNumber, int pageSize, int totalItemCount)
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
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            int numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount ? TotalItemCount : numberOfLastItemOnPage;
            SetPages();
        }

        public SortedSet<int> Pages { get; protected set; }

        public int MaximumPageNumbersToDisplay { get; } = 8;

        public void SetPages(int count = 8)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, "The number of page links must be greater than zero.");
            }

            if (Pages == null)
            {
                Pages = new SortedSet<int>();
            }
            else
            {
                Pages.Clear();
            }

            int halfCount = count / 2;
            int pageNumber = PageNumber;
            int interaction = halfCount;

            while (interaction >= 0 && pageNumber > 0)
            {
                Pages.Add(pageNumber--);
                interaction--;
            }

            interaction = 0;
            pageNumber = PageNumber;

            while (interaction < halfCount && pageNumber < PageCount)
            {
                Pages.Add(++pageNumber);
                interaction++;
            }
        }

        public int Count
        {
            get { return Subset.Count; }
        }

        public T this[int index]
        {
            get { return Subset[index]; }
            set { Subset[index] = value; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(T item)
        {
            Subset.Add(item);
        }

        public void Clear()
        {
            Subset.Clear();
        }

        public bool Contains(T item)
        {
            return Subset.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Subset.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return Subset.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Subset.Insert(index, item);
        }

        public bool Remove(T item)
        {
            return Subset.Remove(item);
        }

        public void RemoveAt(int index)
        {
            Subset.RemoveAt(index);
        }

        public IPagedList GetMetaData()
        {
            return new PagedListMetaData(this);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Subset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Subset.GetEnumerator();
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        object IList.this[int index]
        {
            get { return Subset[index]; }
            set { Subset[index] = (T)value; }
        }

        int IList.Add(object value)
        {
            Subset.Add((T)value);
            return Subset.Count - 1;
        }

        bool IList.Contains(object value)
        {
            return value is T item && Subset.Contains(item);
        }

        int IList.IndexOf(object value)
        {
            return value is T item ? Subset.IndexOf(item) : -1;
        }

        void IList.Insert(int index, object value)
        {
            Subset.Insert(index, (T)value);
        }

        void IList.Remove(object value)
        {
            if (value is T item)
            {
                Subset.Remove(item);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)Subset).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)Subset).SyncRoot; }
        }
    }
}
