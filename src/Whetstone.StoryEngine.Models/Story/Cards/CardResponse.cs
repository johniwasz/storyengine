using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using YamlDotNet.Serialization;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Models.Story.Cards
{
    [JsonObject(Title = "CardResponse")]
    //[Table("LocResponse")]
    [DataContract]
    public class CardResponse
    {
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [YamlMember(Alias = "speechClient")]
        [JsonProperty("speechClient", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Client? SpeechClient { get; set; }

        [DataMember]
        [YamlMember(Alias = "cardTitle")]
        [JsonProperty("cardTitle", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string CardTitle { get; set; }

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



        /// <summary>
        /// Buttons are only applicable to Google Card responses.
        /// </summary>
        [YamlMember(Alias = "buttons")]
        [DataMember()]
        [JsonProperty("buttons", NullValueHandling = NullValueHandling.Ignore)]
        public List<CardButton> Buttons { get; set; }


        
    }
}
