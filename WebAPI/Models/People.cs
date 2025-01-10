using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
namespace WebAPI.Models
{
   public class People
   {
      [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
      //[BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
      public string Id { get; set; } = string.Empty!;

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
