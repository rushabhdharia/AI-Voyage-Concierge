using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Security.Claims;
using AI_Voyage_Concierge.Entities;
using AI_Voyage_Concierge.Data;
using MongoDB.Driver;
using System.Text.Json;
using AI_Voyage_Concierge.DTO;
using Microsoft.AspNetCore.Authorization;

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
            
            var geminiResponse = JsonSerializer.Deserialize<GeminiResponseDTO>(responseBody);

            if (geminiResponse is null) throw new NullReferenceException();

            return geminiResponse.candidates[0].content.parts[0].text;
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

        private static string CreateInformationRequest(InformationDTO informationDto)
        {
            var message = $"Provide the following information for {informationDto.Location}.";

            foreach (var informationType in informationDto.InformationTypes)
            {
                switch (informationType)
                {
                    case InformationType.FunFact:
                        message += "Fun Facts, ";
                        break;
                    case InformationType.History:
                        message += "History,  ";
                        break;
                    case InformationType.LocalCustomsAndTraditions:
                        message += "Local Customs and Traditions, ";
                        break;
                    case InformationType.HiddenGems:
                        message += "Hidden Gems, ";
                        break;
                    case InformationType.CulturalFactsAndSignificance:
                        message += "Cultural Facts and Significance, ";
                        break;
                    case InformationType.CurrentOwnership:
                        message += "Current Ownership, ";
                        break;
                    case InformationType.PreviousOwnerships:
                        message += "Previous Ownerships, ";
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }
            }
            
            
            if (!string.IsNullOrWhiteSpace(informationDto.FreeformText))
            {
                message += $"Additionally, {informationDto.FreeformText}.";
            }

            return message;
        }

        private static string CreateJsonRequest(Conversation conversation, string? freeformText)
        {
            Root root = new();
            
            root.contents = new List<Content>();
            
            // add history
            foreach (var message in conversation.Messages)
            {
                var previousContent = new Content
                {
                    role = message.Role,
                    parts =
                    [
                        new Part { text = message.MessageValue }
                    ]
                };
                root.contents.Add(previousContent);
            }
            
            Content content = new()
            {
                role = "user",
                parts =
                [
                    new Part { text = freeformText ?? throw new ArgumentNullException(nameof(freeformText)) }
                ]
            };
            root.contents.Add(content);

            return JsonSerializer.Serialize(root);
        }

        [Authorize]
        [HttpPost(Name = "GetTravelItinerary")]
        public async Task<ActionResult<CurrentConversation>> GetTravelItinerary(ItineraryDto itineraryDto)
        {
            /*
             * 1. Different Locations
             * 2. Number of Days
             * 3. Freeform text for explanation
             */
            string chatMessage, jsonRequest;
            var currentConversation = new CurrentConversation();
            
            //Create message to be sent to Gemini
            if (!string.IsNullOrWhiteSpace(itineraryDto.ConversationId))
            {
                // get chat history 
                var previousConversation = await GetConversationHistoryById(itineraryDto.ConversationId);
                
                // build json request
                chatMessage = itineraryDto.FreeformText ?? throw new InvalidOperationException();
                jsonRequest = CreateJsonRequest(previousConversation, chatMessage);
            }
            else
            {
                chatMessage = CreateMessage(itineraryDto);
                // build json request
                jsonRequest = CreateJsonRequest(chatMessage);
            }
            
            // send request to gemini
            currentConversation.Response = await SendRequestToGemini(jsonRequest);

            var userMessage = new Message
            {
                Role = "user",
                MessageValue = chatMessage
            };

            var modelMessage = new Message
            {
                Role = "model",
                MessageValue = currentConversation.Response,
            };

            // store chat
            if (string.IsNullOrEmpty(itineraryDto.ConversationId))
            {
                // Create new conversation
                var conversation = new Conversation
                {
                    UserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Messages = [userMessage, modelMessage],
                    ConversationType = ConversationType.Itinerary
                };
                currentConversation.ConversationId = await CreateConversation(conversation);
            }
            else
            {
                // update conversation
                currentConversation.ConversationId = itineraryDto.ConversationId;
                // update conversation in mongodb document

                var messages = new List<Message>();
                messages.Add(userMessage);
                messages.Add(modelMessage);
                
                await UpdateConversation(itineraryDto.ConversationId, messages);
            }
            
            // return response to user
            return currentConversation;
        }
        
        [Authorize]
        [HttpPost(Name = "GetInformationAboutLocation")]
        public async Task<ActionResult<CurrentConversation>> GetInformationAboutLocation(InformationDTO informationDto)
        {
            /*
             * 1. Ask Questions to Bot
             */
             string chatMessage, jsonRequest;
            var currentConversation = new CurrentConversation();
            
            //Create message to be sent to Gemini
            if (!string.IsNullOrWhiteSpace(informationDto.ConversationId))
            {
                // get chat history 
                var previousConversation = await GetConversationHistoryById(informationDto.ConversationId);
                
                // build json request
                chatMessage = informationDto.FreeformText ?? throw new InvalidOperationException();
                jsonRequest = CreateJsonRequest(previousConversation, chatMessage);
            }
            else
            {
                chatMessage = CreateInformationRequest(informationDto);
                // build json request
                jsonRequest = CreateJsonRequest(chatMessage);
            }
            
            // send request to gemini
            currentConversation.Response = await SendRequestToGemini(jsonRequest);

            var userMessage = new Message
            {
                Role = "user",
                MessageValue = chatMessage
            };

            var modelMessage = new Message
            {
                Role = "model",
                MessageValue = currentConversation.Response,
            };

            // store chat
            if (string.IsNullOrEmpty(informationDto.ConversationId))
            {
                // Create new conversation
                var conversation = new Conversation
                {
                    UserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value, 
                    Messages = [userMessage, modelMessage],
                    ConversationType = ConversationType.Information
                };
                currentConversation.ConversationId = await CreateConversation(conversation);
            }
            else
            {
                // update conversation
                currentConversation.ConversationId = informationDto.ConversationId;
                // update conversation in mongodb document

                var messages = new List<Message>();
                messages.Add(userMessage);
                messages.Add(modelMessage);
                
                await UpdateConversation(informationDto.ConversationId, messages);
            }
            
            // return response to user
            return currentConversation;
        }
        
        /// <summary>
        /// Get a list of all conversations for the given user
        /// </summary>
        /// <returns>A list of conversations</returns>
        [Authorize]
        [HttpGet(Name = "GetConversationHistory")]
        public async Task<IEnumerable<Conversation>> GetConversationHistory()
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var filter = Builders<Conversation>.Filter.Eq("user_email", userEmail);
            var conversationsList = await _conversations.Find(filter).ToListAsync();

            // Find a better wat to get only the first message for each conversation
            // Possible solution is to use MongoDB Aggregation pipeline
            foreach (var conversation in conversationsList)
            {
                var message = conversation.Messages[0];
                conversation.Messages = [message];
            }
            
            return conversationsList;
        }


        [Authorize]
        [HttpGet("{conversationId}", Name = "GetConversationById")]
        public async Task<List<Message>> GetConversationById(string conversationId)
        {
            var conversation = await GetConversationHistoryById(conversationId);
            return conversation.Messages;
        }
        
        /// <summary>
        /// Get a single conversation by its id
        /// </summary>
        /// <param name="conversationId">The id of the conversation</param>
        /// <returns>The conversation or null if no conversation is found</returns>
        private async Task<Conversation> GetConversationHistoryById(string conversationId)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var filterEmail = Builders<Conversation>.Filter.Eq("user_email", userEmail);
            var filterConversationId = Builders<Conversation>.Filter.Eq(x => x.Id, conversationId);
            return await _conversations.Find(filterConversationId).FirstOrDefaultAsync();
        }


        private async Task<string?> CreateConversation(Conversation conversation)
        {
             await _conversations.InsertOneAsync(conversation);
             return conversation.Id;
        }
        
        private async Task UpdateConversation(string conversationId, List<Message> messages)
        {
            var filter = Builders<Conversation>.Filter.Eq(x => x.Id, conversationId);
            //var update = Builders<Conversation>.Update.Push<Message>(x => x.Messages, message);
            
            var conversation = await _conversations.Find(filter).FirstOrDefaultAsync();
            foreach (var message in messages)
            {
                conversation.Messages.Add(message);
            }
            
            await _conversations.ReplaceOneAsync(filter, conversation);
        }
    }
}
