using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class Conversation
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    [BsonElement("user_email"), BsonRepresentation(BsonType.String)]
    public required string UserEmail { get; set; }
    
    [BsonElement("messages")]
    public required List<Message> Messages { get; set; }
    
    [BsonElement("conversation_type"), BsonRepresentation(BsonType.Int32)]
    public required ConversationType ConversationType { get; set; }
}


public enum ConversationType
{
    Itinerary = 0,
    Information = 1
}