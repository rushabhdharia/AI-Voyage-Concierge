using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class User
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    
    [BsonElement("name"), BsonRepresentation(BsonType.String)]
    public string Name { get; set; }

    [BsonElement("email"), BsonRepresentation(BsonType.String)]
    public string Email { get; set; }
    
    [BsonElement("password_salt"), BsonRepresentation(BsonType.String)]
    public string PasswordSalt { get; set; }
    
    [BsonElement("password_hash"), BsonRepresentation(BsonType.String)]
    public string PasswordHash { get; set; }
}