using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class User
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public required string Id { get; set; }
    
    [BsonElement("name"), BsonRepresentation(BsonType.String)]
    public required string Name { get; set; }

    [BsonElement("email"), BsonRepresentation(BsonType.String)]
    public required string Email { get; set; }
    
    [BsonElement("password_salt"), BsonRepresentation(BsonType.String)]
    public required string PasswordSalt { get; set; }
    
    [BsonElement("password_hash"), BsonRepresentation(BsonType.String)]
    public required string PasswordHash { get; set; }
}