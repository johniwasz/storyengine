using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Story.Ssml;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;
using Whetstone.StoryEngine.Models.Data;
using Newtonsoft.Json.Converters;

namespace Whetstone.StoryEngine.Models.Story
{

   
    public class ClientSpeechFragments
    {

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("speechClient", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public Client? SpeechClient { get; set; }

        
        [JsonProperty("speechFragments", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public List<SpeechFragment> SpeechFragments { get; set; }


    }

}
