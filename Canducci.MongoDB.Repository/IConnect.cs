﻿using MongoDB.Driver;

namespace Canducci.MongoDB.Repository
{
    public interface IConnect
    {
        IMongoCollection<T> Collection<T>(string CollectionName);
    }
}