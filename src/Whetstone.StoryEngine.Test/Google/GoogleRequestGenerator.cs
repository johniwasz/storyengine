using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Test.Google
{
    public class GoogleRequestGenerator
    {
        private string _apiKey = null;
        private Uri _apiUri = null;
        private JsonParser _jsonParser;


        public GoogleRequestGenerator(Uri apiUri, string apiKey)
        {
            _apiUri = apiUri;
            _apiKey = apiKey;
            _jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));

        }


        public async Task<WebhookResponse> GetResponseAsync(WebhookRequest request)
        {
            WebhookResponse resp = null;
            // WebhookRequest request = jsonParser.Parse<WebhookRequest>(requestText);


            using (HttpClient client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 29);

                string sendText = request.ToString();

                StringContent httpString = new StringContent(sendText, Encoding.UTF8, "application/json");
                httpString.Headers.Add("x-api-key", _apiKey);
                HttpResponseMessage respMsg = await client.PostAsync(_apiUri, httpString);


                string responseText = await respMsg.Content.ReadAsStringAsync();

                resp = _jsonParser.Parse<WebhookResponse>(responseText);

            }

            string retPayload = resp.Payload.ToString();

            return resp;
        }

    }
}
