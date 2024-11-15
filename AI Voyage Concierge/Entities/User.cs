using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AI_Voyage_Concierge.Entities;

public class User
{
    [BsonId]
    [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    [BsonElement("name"), BsonRepresentation(BsonType.String)]
    public string? Name { get; set; }

    [BsonElement("email"), BsonRepresentation(BsonType.String)]
    public string? Email { get; set; }
    
    //refactor once salt and hash are implemented
    [BsonElement("password"), BsonRepresentation(BsonType.String)]
    public string? Password { get; set; }
    
    //implement later
    // [BsonElement("password_salt"), BsonRepresentation(BsonType.String)]
    // public required string PasswordSalt { get; set; }
    
    //implement later
    // [BsonElement("password_hash"), BsonRepresentation(BsonType.String)]
    // public required string PasswordHash { get; set; }
}