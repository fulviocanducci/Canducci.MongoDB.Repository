using Canducci.Pagination.Interfaces;

namespace Canducci.MongoDB.Repository.Paged
{
    public interface IPagedList<T> : IPagedList, IPaginated<T>
    {
        IPagedList GetMetaData();
    }

    public interface IPagedList : IPaginatedRest
    {
    }
}
