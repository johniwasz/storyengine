using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class AppMapping
    {
        /// <summary>
        /// This is the application id provided by the host of the voice application. 
        /// </summary>
        /// <remarks>
        /// For Alexa, this is the Skill ID. For Google, this is the DialogFlow Agent name.
        /// </remarks>
        [JsonProperty(PropertyName = "appId")]
        public string AppId { get; set; }

        /// <summary>
        /// This is the id of the title configured in the StoryEngine environment.
        /// </summary>
        [JsonProperty(PropertyName = "titleId")]
        public string TitleId { get; set; }

        /// <summary>
        /// This is the version to associate with the title.
        /// </summary>
        [JsonProperty(PropertyName = "version")]
        public string Version { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "clientType")]
        public Client ClientType { get; set; }

    }
}
