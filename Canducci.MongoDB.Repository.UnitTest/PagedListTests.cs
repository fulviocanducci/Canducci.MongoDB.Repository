using System;
using System.Collections.Generic;
using Canducci.MongoDB.Repository.Paged;
using Canducci.Pagination.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class PagedListTests
    {
        [TestMethod]
        public void Constructor_ShouldPopulateItemsAndMetadata()
        {
            List<string> items = new List<string> { "item-4", "item-5", "item-6" };

            PagedList<string> pagedList = new PagedList<string>(items, 2, 3, 10);

            Assert.AreEqual(3, pagedList.Count);
            Assert.AreEqual("item-4", pagedList[0]);
            Assert.AreEqual(10, pagedList.TotalItemCount);
            Assert.AreEqual(4, pagedList.PageCount);
            Assert.AreEqual(2, pagedList.PageNumber);
            Assert.AreEqual(3, pagedList.PageSize);
            Assert.IsTrue(pagedList.HasPreviousPage);
            Assert.IsTrue(pagedList.HasNextPage);
            Assert.AreEqual(4, pagedList.FirstItemOnPage);
            Assert.AreEqual(6, pagedList.LastItemOnPage);
        }

        [TestMethod]
        public void Constructor_ShouldImplementCanducciPaginatedContract()
        {
            IPaginated<string> paginated = new PagedList<string>(
                new List<string> { "item-1", "item-2" },
                1,
                2,
                10);

            Assert.AreEqual(5, paginated.PageCount);
            Assert.AreEqual(8, paginated.MaximumPageNumbersToDisplay);
            Assert.IsTrue(paginated.IsFirstPage);
            Assert.IsTrue(paginated.HasNextPage);
            Assert.IsTrue(paginated.Pages.Contains(1));
            Assert.IsTrue(paginated.Pages.Contains(5));
        }

        [TestMethod]
        public void SetPages_ShouldRebuildVisiblePageNumbers()
        {
            IPaginated<string> paginated = new PagedList<string>(
                new List<string> { "item-5", "item-6" },
                5,
                2,
                20);

            paginated.SetPages(4);

            CollectionAssert.AreEqual(
                new[] { 3, 4, 5, 6, 7 },
                new List<int>(paginated.Pages));
        }

        [TestMethod]
        public void PagedList_ShouldBeUsableAsMvcPaginationModel()
        {
            PagedList<string> pagedList = new PagedList<string>(
                new List<string> { "item-1" },
                1,
                10,
                20);

            IPaginated<string> mvcModel = pagedList;

            Assert.AreEqual(2, mvcModel.PageCount);
            Assert.AreEqual(1, mvcModel.PageNumber);
        }

        [TestMethod]
        public void Constructor_ShouldRejectInvalidPageNumber()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new PagedList<string>(new List<string>(), 0, 10, 0));
        }

        [TestMethod]
        public void Constructor_ShouldRejectInvalidPageSize()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(
                () => new PagedList<string>(new List<string>(), 1, 0, 0));
        }
    }
}
