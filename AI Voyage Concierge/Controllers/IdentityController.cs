using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace AI_Voyage_Concierge.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class IdentityController : Controller
{
    private readonly IMongoCollection<User> _users;
    private readonly IConfiguration _configuration;
    // Constructor
    public IdentityController(MongoDbService mongoDbService, IConfiguration configuration)
    {
        _users = mongoDbService.Database.GetCollection<User>("users");
        _configuration = configuration;
    }
    [HttpGet, AllowAnonymous]
    public async Task<IEnumerable<User>> Get()
    {
        return await _users.Find(_ => true).ToListAsync();
    }
    
    [HttpPost, AllowAnonymous]
    public async Task<User> Create(User user)
    {
        await _users.InsertOneAsync(user);
        return user;
    }
    
    [HttpGet, AllowAnonymous]
    public async Task<User> GetUserByEmail(string email)
    {
        return await _users.Find(x => x.Email == email).FirstOrDefaultAsync();
    }
    
    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> Authenticate (User user)
    {
        var currentUser = await _users.Find(x => x.Email == user.Email && x.Password == user.Password).FirstOrDefaultAsync();
        if (currentUser == null) throw new Exception("User not found");
        
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtKey").Value ?? throw new InvalidOperationException());
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, currentUser.Email ?? throw new InvalidOperationException())
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        
        return Ok(tokenString);

    }
} 