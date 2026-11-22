using Canducci.MongoDB.Repository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;

namespace Web11.Models
{
    [BsonCollectionName("people")]
    public class People
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; } = string.Empty;

        [BsonElement("name")]
        [BsonRequired]
        [Required(ErrorMessage = "Digite o nome completo")]
        public string Name { get; set; } = string.Empty;
    }
}
