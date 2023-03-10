using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Integration
{
    public static class IntegrationExtensions
    {

        public static string ToJsonString(this SearchRequest req)
        {
            JsonSerializerSettings serSettings = new JsonSerializerSettings();

            serSettings.Formatting = Formatting.Indented;
            serSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();


            string searchReqText = JsonConvert.SerializeObject(req, serSettings);
            return searchReqText;
        }

    }
}
