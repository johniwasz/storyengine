using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{
    [JsonObject(Title ="LocalizedResponse")]
    [Table("LocResponse")]
    [DataContract]
    public class LocalizedResponse
    {

        /// <summary>
        /// If null, then then localized response applies to all locales.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "locale")]
        [JsonProperty("locale", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Locale { get; set; }


        [DataMember]
        [YamlMember(Alias = "cardTitle")]
        [JsonProperty("cardTitle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CardTitle { get; set; }


        [DataMember]
        [YamlMember(Alias = "sendCardResponse")]
        [JsonProperty("sendCardResponse", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendCardResponse { get; set; }

        [DataMember]
        [YamlMember(Alias = "cardResponses")]
        [JsonProperty("cardResponses", NullValueHandling = NullValueHandling.Ignore)]
        public List<CardResponse> CardResponses { get; set; }

        /// <summary>
        /// This is the story text that is returned to the client if the client supports text only.
        /// </summary>
        /// <remarks>
        /// If this is coming back to Alexa and the SpeechResponse for Alexa is set, then the SpeechResponse is used.
        /// In Alexa, this text response will always display in a card response.
        /// </remarks>
        [DataMember]
        [YamlMember(Alias = "textFragments")]
        [JsonProperty("textFragments", NullValueHandling = NullValueHandling.Ignore)]
        public List<TextFragmentBase> TextFragments { get; set; }


        [DataMember]
        [JsonProperty("repromptTextResponse", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string RepromptTextResponse { get; set; }


        /// <summary>
        /// This is the standard SSML response for audio appliances like Alexa and Google Home.
        /// </summary>
        /// <remarks>If this is null then the text response is returned and 
        /// just wrapped in a speak tag to be SSML compliant.</remarks>
        [DataMember]
        [YamlMember(Alias = "clientResponses")]
        [JsonProperty("clientResponses", NullValueHandling = NullValueHandling.Ignore)]
        public List<ClientSpeechFragments> SpeechResponses { get; set; }


     
        [DataMember]
        [YamlMember(Alias = "clientSpeechReprompts")]
        [JsonProperty("clientSpeechReprompts", NullValueHandling = NullValueHandling.Ignore)]
        public List<ClientSpeechFragments> RepromptSpeechResponses { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        [YamlMember(Alias = "smallImageFile")]
        [DataMember()]
        [JsonProperty("smallImageFile", NullValueHandling = NullValueHandling.Ignore)]
        public string SmallImageFile { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        [YamlMember(Alias = "largeImageFile")]
        [DataMember()]
        [JsonProperty("largeImageFile", NullValueHandling = NullValueHandling.Ignore)]
        public string LargeImageFile { get; set; }


    }
}
