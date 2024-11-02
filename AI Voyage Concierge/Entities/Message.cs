using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class Message
{
    [BsonElement("role"), BsonRepresentation(BsonType.String)]
    public required string Role { get; set; }

    [BsonElement("message"), BsonRepresentation(BsonType.String)]
    public required string MessageValue { get; set; }
}