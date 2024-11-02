using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class Conversation
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    
    //redundant need to refactor and remove
    [BsonElement("conversation_id"), BsonRepresentation(BsonType.Int32)]
    public int? ConversationId { get; set; }
    
    [BsonElement("user_email"), BsonRepresentation(BsonType.String)]
    public required string UserEmail { get; set; }
    
    [BsonElement("total_messages"), BsonRepresentation(BsonType.Int32)]
    public required int TotalMessages { get; set; }
    
    [BsonElement("messages")]
    public required List<Message> Messages { get; set; }
}