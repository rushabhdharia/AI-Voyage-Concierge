using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;

namespace AI_Voyage_Concierge.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly ILogger<AIController> _logger;
        private readonly IConfiguration _configuration;
        private readonly string geminiUrl;


        /// <summary>
        /// Constructor for AIController.
        /// </summary>
        /// <param name="logger">Logger instance.</param>
        /// <param name="configuration">Configuration instance.</param>
        public AIController(ILogger<AIController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            var GeminiSection = _configuration?.GetSection("Gemini");
            geminiUrl = GeminiSection?["url"] + GeminiSection?["key"];
        }


        private async Task<string> SendRequestToGemini(string content)
        {
            HttpClient client = new();          

            HttpRequestMessage request = new(HttpMethod.Post, geminiUrl)
            {
                Content = new StringContent(content)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }


        [HttpPost(Name = "GetTravelItenary")]
        public string GetTravelItenary()
        {
            /*
             * 1. Different Locations
             * 2. Number of Days
             * 3. Freeform text for explaination
             */


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



    }
}
