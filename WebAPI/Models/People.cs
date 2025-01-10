using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace WebAPI.Models
{
   public class People
   {
      [BsonId()]
      [BsonGuidRepresentation(GuidRepresentation.CSharpLegacy)]
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
