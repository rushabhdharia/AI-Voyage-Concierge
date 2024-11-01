using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace AI_Voyage_Concierge.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class IdentityController : Controller
{
    private readonly IMongoCollection<User> _users;
    // Constructor
    public IdentityController(MongoDBService mongoDbService)
    {
        _users = mongoDbService.Database.GetCollection<User>("users");
    }
    [HttpGet]
    public async Task<IEnumerable<User>> Get()
    {
        return await _users.Find(_ => true).ToListAsync();
    }
}