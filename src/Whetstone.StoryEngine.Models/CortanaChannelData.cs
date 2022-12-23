using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Whetstone.StoryEngine.Models
{
    public class CortanaChannelData
    {

        [JsonProperty("skillId")]
        public string SkillId { get; set; }


        [JsonProperty("skillProductId")]
        public string SkillProductId { get; set; }

        [JsonProperty("isDebug")]
        public bool IsDebug { get; set; }
    }
}
