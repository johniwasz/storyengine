using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{
    [DataContract]
    [JsonObject("Localized intent model", IsReference = false)]
    public class LocalizedIntent : ILocalizedItem
    {

        [DataMember]
        [JsonProperty("locale", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [YamlMember]
        public string Locale { get; set; }

        /// <summary>
        /// When using a chat bot, this is the text prompt presented to the end user.
        /// </summary>
        [DataMember]
        [JsonProperty("plainTextPrompt", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [YamlMember]
        public string PlainTextPrompt { get; set; }


        /// <summary>
        /// List of voice inputs words and phrases that map to the intent.
        /// </summary>
        [DataMember]
        [JsonProperty("utterances")]
        [YamlMember]
        public List<string> Utterances { get; set; }
    }
}
