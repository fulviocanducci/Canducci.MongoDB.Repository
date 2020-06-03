using System;
using System.Runtime.InteropServices;

namespace Canducci.MongoDB.Repository.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    [ComVisible(true)]
    public class BsonCollectionName : Attribute
    {
        public string Name { get; }
        public BsonCollectionName(string name)
        {
            Name = name;
        }
    }
}
