using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class Message
{
    [BsonElement("role"), BsonRepresentation(BsonType.String)]
    public string Role { get; set; }

    [BsonElement("message"), BsonRepresentation(BsonType.String)]
    public string MessageValue { get; set; }
}