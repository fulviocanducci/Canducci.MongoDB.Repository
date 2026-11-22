using MongoDB.Driver;

namespace Canducci.MongoDB.Repository.Interfaces
{
    public interface IRepositoryMongoCollection<T> where T : class, new()
    {
        #region IMongoCollection
        IMongoCollection<T> Collection { get; }
        #endregion
    }
}