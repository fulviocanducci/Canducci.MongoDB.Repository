using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{
    [BsonCollectionName("person")]
    public class Person
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }

        [BsonElement("name")]
        [BsonRequired()]
        [Required()]
        public string Name { get; set; }
    }
}
