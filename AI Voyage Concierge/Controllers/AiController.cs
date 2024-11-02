using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Data;
using MongoDB.Driver;

namespace AI_Voyage_Concierge.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly string _geminiUrl;
        private readonly IMongoCollection<Conversation> _conversations;
        
        public AiController(IConfiguration configuration, MongoDbService mongoDbService)
        {
            _conversations = mongoDbService.Database.GetCollection<Conversation>("conversations");

            var geminiSection = configuration.GetSection("Gemini");
            _geminiUrl = geminiSection["url"] + geminiSection["APIkey"];
        }


        private async Task<string> SendRequestToGemini(string content)
        {
            HttpClient client = new();          

            HttpRequestMessage request = new(HttpMethod.Post, _geminiUrl)
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


        [HttpPost(Name = "GetTravelItinerary")]
        public async Task<CurrentConversation> GetTravelItinerary(string[] locations, int numberOfDays, string freeformText, string conversationId="")
        {
            /*
             * 1. Different Locations
             * 2. Number of Days
             * 3. Freeform text for explaination
             */
            var currentConversation = new CurrentConversation();
            if (!string.IsNullOrWhiteSpace(conversationId))
            {
                // get chat history 
                
            }
            
            // build json request
            var jsonRequest = await CreateJsonRequest();
            
            // send request to gemini
            currentConversation.Response = await SendRequestToGemini(jsonRequest);

            // store chat
            if (string.IsNullOrEmpty(conversationId))
            {
                // Create new conversation
            }
            else
            {
                // update conversation
                currentConversation.ConversationId = conversationId;
            }
            
            // return response to user
            return currentConversation;
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
