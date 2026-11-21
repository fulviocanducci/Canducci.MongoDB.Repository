using Canducci.Pagination.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Canducci.MongoDB.Repository
{
   public static class Extensions
   {
      public static IPaginated<T> ToPagedList<T>(this IQueryable<T> superset, int pageNumber, int pageSize)
      {
         if (superset == null)
         {
            throw new ArgumentNullException(nameof(superset));
         }
         if (pageNumber < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageNumber), pageNumber, "PageNumber cannot be below 1.");
         }
         if (pageSize < 1)
         {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
         }

         int totalItemCount = superset.Count();
         List<T> items = superset
             .Skip(checked((pageNumber - 1) * pageSize))
             .Take(pageSize)
             .ToList();

         return new PagedList<T>(items, pageNumber, pageSize, totalItemCount);
      }

      public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> superset, int numberOfPages)
      {
         return superset
             .Select((item, index) => new { index, item })
             .GroupBy(x => x.index % numberOfPages)
             .Select(x => x.Select(y => y.item));
      }

      public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> superset, int pageSize)
      {
         if (superset.Count() < pageSize)
         {
            yield return superset;
         }
         else
         {
            var numberOfPages = Math.Ceiling(superset.Count() / (double)pageSize);
            for (var i = 0; i < numberOfPages; i++)
               yield return superset.Skip(pageSize * i).Take(pageSize);
         }
      }
   }
}
