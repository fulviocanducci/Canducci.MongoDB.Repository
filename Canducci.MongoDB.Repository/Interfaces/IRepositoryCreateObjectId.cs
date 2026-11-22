using MongoDB.Bson;

namespace Canducci.MongoDB.Repository
{
    public interface IRepositoryCreateObjectId<T> where T : class, new()
    {
        #region CreateObjectId
        ObjectId CreateObjectId(string value);
        #endregion
    }
}