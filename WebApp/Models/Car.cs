using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Models
{

    [BsonCollectionName("car")]
    public class Car
    {
        [BsonId()]
        public ObjectId Id { get; set; }

        [BsonElement("name")]
        [BsonRequired()]
        [Required()]
        public string Name { get; set; }
    }

}
