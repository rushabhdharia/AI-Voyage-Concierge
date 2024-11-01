using MongoDB.Driver;

namespace AI_Voyage_Concierge.Services;

public class MongoDBService
{
    private readonly IConfiguration _configuration;
    private readonly IMongoDatabase _mongoDatabase;
    
    public MongoDBService(
        IConfiguration configuration)
    {
        _configuration = configuration;
        
        var connectionString = _configuration.GetConnectionString("DBConnection");
        var mongoUrl = new MongoUrl(connectionString);
        
        var mongoClient = new MongoClient(mongoUrl);
        _mongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);
    }
    
    public IMongoDatabase Database => _mongoDatabase;
}