using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Data;
using MongoDB.Driver;
using System.Text.Json;
using AI_Voyage_Concierge.DTO;

namespace AI_Voyage_Concierge.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly string _geminiUrl;
        private readonly IMongoCollection<Conversation> _conversations;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration">IConfiguration to read appsettings from</param>
        /// <param name="mongoDbService">MongoDbService to get a collection of conversations</param>
        public AiController(IConfiguration configuration, MongoDbService mongoDbService)
        {
            _conversations = mongoDbService.Database.GetCollection<Conversation>("conversations");

            var geminiSection = configuration.GetSection("Gemini");
            _geminiUrl = geminiSection["url"] + geminiSection["APIkey"];
        }


        /// <summary>
        /// Send a request to the Gemini API with the given content
        /// </summary>
        /// <param name="content">The content of the request</param>
        /// <returns>The response from Gemini</returns>
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
        
        private static string CreateJsonRequest(string message)
        {
            // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
            Root root = new();
            Content content = new();
            content.role = "user";
            content.parts =
            [
                new Part { text = message }
            ];
            root.contents =
            [
                content
            ];

            return JsonSerializer.Serialize(root);
        }
        
        /// <summary>
        /// Creates a message to be sent to the Gemini API to request a travel itinerary
        /// </summary>
        /// /// <param name="itineraryDto"></param>
        /// <returns>The message to be sent to Gemini</returns>
        private static string CreateMessage(ItineraryDto itineraryDto)
        {
            var message = $"Create a travel itinerary for {itineraryDto.NumberOfDays} days.";

            if (itineraryDto.Locations is { Length: > 1 })
            {
                message = $"I want to travel to the following locations - {string.Join(", ", itineraryDto.Locations)}.";
            }
            else if (itineraryDto.Locations is { Length: 1 })
            {
                message = $"I want to travel to {itineraryDto.Locations[0]}.";
            }
            
            if (!string.IsNullOrWhiteSpace(itineraryDto.FreeformText))
            {
                message += $"Additional Considerations - {itineraryDto.FreeformText}.";
            }

            return message;
        }

        private static string CreateJsonRequest(Conversation conversation, string? freeformText)
        {
            Root root = new();
            Content content = new();
            
            // add history
            foreach (var message in conversation.Messages)
            {
                content.role = message.Role;
                content.parts =
                [
                    new Part { text = message.MessageValue }
                ];
                root.contents.Add(content);
            }
            
            content.role = "user";
            content.parts =
            [
                new Part { text = freeformText ?? throw new ArgumentNullException(nameof(freeformText)) }
            ];
            root.contents.Add(content);

            return JsonSerializer.Serialize(root);
        }

        [HttpPost(Name = "GetTravelItinerary")]
        public async Task<ActionResult<CurrentConversation>> GetTravelItinerary(ItineraryDto itineraryDto)
        {
            /*
             * 1. Different Locations
             * 2. Number of Days
             * 3. Freeform text for explanation
             */
            string message, jsonRequest;
            var currentConversation = new CurrentConversation();
            
            //Create message to be sent to Gemini
            if (!string.IsNullOrWhiteSpace(itineraryDto.ConversationId))
            {
                // get chat history 
                var previousConversation = await GetConversationHistoryById(itineraryDto.ConversationId);
                
                // build json request
                jsonRequest = CreateJsonRequest(previousConversation, itineraryDto.FreeformText);
            }
            else
            {
                message = CreateMessage(itineraryDto);
                // build json request
                jsonRequest = CreateJsonRequest(message);
            }
            
            
            
            // send request to gemini
            currentConversation.Response = await SendRequestToGemini(jsonRequest);
            
            // store chat
            if (string.IsNullOrEmpty(itineraryDto.ConversationId))
            {
                // Create new conversation
                throw new NotImplementedException();
            }
            else
            {
                // update conversation
                currentConversation.ConversationId = itineraryDto.ConversationId;
                // update conversation in mongodb document
                throw new NotImplementedException();
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
        
        /// <summary>
        /// Get a list of all conversations for the given user
        /// </summary>
        /// <param name="userEmail">The email of the user</param>
        /// <returns>A list of conversations</returns>
        [HttpGet(Name = "GetConversationHistory")]
        public async Task<IEnumerable<Conversation>> GetConversationHistory(string userEmail)
        {
            var filter = Builders<Conversation>.Filter.Eq("user_email", userEmail);
            return await _conversations.Find(filter).ToListAsync();
        }

        /// <summary>
        /// Get a single conversation by its id
        /// </summary>
        /// <param name="conversationId">The id of the conversation</param>
        /// <returns>The conversation or null if no conversation is found</returns>
        private async Task<Conversation> GetConversationHistoryById(string conversationId)
        {
            var userEmail = "rdharia@gmail.com"; // replace using claim from jwt
            var filterEmail = Builders<Conversation>.Filter.Eq("user_email", userEmail);
            var filterConversationId = Builders<Conversation>.Filter.Eq("conversation_id", conversationId);
            return await _conversations.Find(filterEmail&filterConversationId).FirstOrDefaultAsync();
        }



    }
}
