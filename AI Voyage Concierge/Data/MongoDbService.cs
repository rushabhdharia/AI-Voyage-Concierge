using MongoDB.Driver;

namespace AI_Voyage_Concierge.Data;

public class MongoDbService
{
    private readonly IMongoDatabase _mongoDatabase;
    
    public MongoDbService(
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DBConnection");
        var mongoUrl = new MongoUrl(connectionString);
        
        var mongoClient = new MongoClient(mongoUrl);
        _mongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
    }
    
    public IMongoDatabase Database => _mongoDatabase;
}