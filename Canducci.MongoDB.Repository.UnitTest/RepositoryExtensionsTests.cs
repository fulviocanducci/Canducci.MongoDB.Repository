using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Canducci.MongoDB.Repository.UnitTest
{
    [TestClass]
    public class RepositoryExtensionsTests
    {
        [TestMethod]
        public void Split_ShouldDistributeItemsAcrossGroups()
        {
            List<List<int>> groups = new[] { 1, 2, 3, 4, 5 }
                .Split(2)
                .Select(group => group.ToList())
                .ToList();

            Assert.AreEqual(2, groups.Count);
            CollectionAssert.AreEqual(new[] { 1, 3, 5 }, groups[0]);
            CollectionAssert.AreEqual(new[] { 2, 4 }, groups[1]);
        }

        [TestMethod]
        public void Partition_ShouldCreateConsecutivePages()
        {
            List<List<int>> pages = new[] { 1, 2, 3, 4, 5 }
                .Partition(2)
                .Select(page => page.ToList())
                .ToList();

            Assert.AreEqual(3, pages.Count);
            CollectionAssert.AreEqual(new[] { 1, 2 }, pages[0]);
            CollectionAssert.AreEqual(new[] { 3, 4 }, pages[1]);
            CollectionAssert.AreEqual(new[] { 5 }, pages[2]);
        }

        [TestMethod]
        public void ToPagedList_ShouldReturnEmptyPageForEmptySource()
        {
            IPaginated<int> result = Enumerable.Empty<int>().AsQueryable().ToPagedList(1, 10);

            Assert.AreEqual(0, result.TotalItemCount);
            Assert.AreEqual(0, result.PageCount);
            Assert.AreEqual(0, ((ICollection<int>)result).Count);
        }

        [TestMethod]
        public void ToPagedList_ShouldRejectNullSource()
        {
            IQueryable<int> source = null;

            Assert.ThrowsException<ArgumentNullException>(() => source.ToPagedList(1, 10));
        }

        [TestMethod]
        public void ToPagedList_ShouldRejectInvalidPageNumber()
        {
            IQueryable<int> source = new[] { 1, 2, 3 }.AsQueryable();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ToPagedList(0, 2));
        }

        [TestMethod]
        public void ToPagedList_ShouldRejectInvalidPageSize()
        {
            IQueryable<int> source = new[] { 1, 2, 3 }.AsQueryable();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => source.ToPagedList(1, 0));
        }
    }
}
