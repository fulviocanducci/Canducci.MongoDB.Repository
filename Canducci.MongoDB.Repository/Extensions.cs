using Canducci.MongoDB.Repository.Paged;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Canducci.MongoDB.Repository
{
   public static class Extensions
   {
      public static IPagedList<T> ToPagedList<T>(this IQueryable<T> superset, int pageNumber, int pageSize)
      {
         return new PagedList<T>(superset, pageNumber, pageSize);
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
