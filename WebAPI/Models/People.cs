using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using BsonIdGeneratorTypes = MongoDB.Bson.Serialization.IdGenerators;
namespace WebAPI.Models
{
   public class People
   {
      [BsonId(IdGenerator = typeof(BsonIdGeneratorTypes.CombGuidGenerator))]
      public Guid Id { get; set; }

      [BsonElement("name")]
      public string Name { get; set; } = string.Empty!;

      [BsonElement("created_at")]
      public DateTime CreatedAt { get; set; }

      [BsonElement("status")]
      public bool Status { get; set; }

      [BsonElement("value")]
      public decimal Value { get; set; }
   }
}
