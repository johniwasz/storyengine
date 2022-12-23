using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{
    [DataContract]
    [Table("LocResponse")]
    public  class DataLocalizedResponse
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [YamlIgnore]
        [DataMember]
        public long? Id { get; set; }

        /// <summary>
        /// If null, then then localized response applies to all locales.
        /// </summary>
        [DataMember]
        public string Locale { get; set; }


        [DataMember]
        public string CardTitle { get; set; }


        [DataMember]
        [JsonProperty("sendCardResponse", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? SendCardResponse { get; set; }

        /// <summary>
        /// This is the story text that is returned to the client if the client supports text only.
        /// </summary>
        /// <remarks>
        /// If this is coming back to Alexa and the SpeechResponse for Alexa is set, then the SpeechResponse is used.
        /// In Alexa, this text response will always display in a card response.
        /// </remarks>
        [DataMember]
        public List<TextFragmentBase> TextFragments { get; set; }


        [DataMember]
        [JsonProperty("repromptTextResponse", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string RepromptTextResponse { get; set; }


        /// <summary>
        /// This is the standard SSML response for audio appliances like Alexa and Google Home.
        /// </summary>
        /// <remarks>If this is null then the text response is returned and 
        /// just wrapped in a speak tag to be SSML compliant.</remarks>
        [DataMember]
        [ForeignKey("LocResponseId")]
        public List<DataClientSpeechFragments> SpeechResponses { get; set; }



        [DataMember]
        [ForeignKey("LocRepromptResponseId")]
        public List<DataClientSpeechFragments> RepromptSpeechResponses { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        [DataMember()]
        public string SmallImageFile { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        [DataMember()]
        public string LargeImageFile { get; set; }

        [NotMapped]
        [YamlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public List<string> GeneratedTextResponse { get; set; }

    }
}
