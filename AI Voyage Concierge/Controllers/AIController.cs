using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Services;
using MongoDB.Driver;

namespace AI_Voyage_Concierge.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string geminiUrl;
        private readonly IMongoCollection<Conversation> _conversations;
        
        public AIController(IConfiguration configuration, MongoDBService mongoDbService)
        {
            _configuration = configuration;
            _conversations = mongoDbService.Database.GetCollection<Conversation>("conversations");

            var GeminiSection = _configuration?.GetSection("Gemini");
            geminiUrl = GeminiSection?["url"] + GeminiSection?["APIkey"];
        }


        private async Task<string> SendRequestToGemini(string content)
        {
            HttpClient client = new();          

            HttpRequestMessage request = new(HttpMethod.Post, geminiUrl)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }

        private async Task<string> CreateJsonRequest()
        {

            return "";
        }


        [HttpPost(Name = "GetTravelItenary")]
        public string GetTravelItenary(string[] locations, int numberOfDays, string freeformText, int previousChatId=-1)
        {
            /*
             * 1. Different Locations
             * 2. Number of Days
             * 3. Freeform text for explaination
             */
            if (previousChatId != -1)
            {
                // get chat history 
            }
            
            // build json request
            
            // send request to gemini

            // store chat
            
            // return response to user
            return "";
        }

        [HttpPost(Name = "GetInformationAboutLocation")]
        public string GetInformationAboutLocation()
        {
            /*
             * 1. Ask Questions to Bot
             */
            return "";
        }

        [HttpPost(Name = "LocationBasedNotifications")]
        public string LocationBasedNotifications()
        {
            /*
             * 1. Get Location and get notifications
             */
            return "";
        }
        
        [HttpGet(Name = "GetConversationHistory")]
        public async Task<IEnumerable<Conversation>> GetConversationHistory(string userEmail)
        {
            var filter = Builders<Conversation>.Filter.Eq("user_email", userEmail);
            return await _conversations.Find(filter).ToListAsync();
        }



    }
}
