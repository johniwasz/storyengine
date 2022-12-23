using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Whetstone.StoryEngine.Bixby.Repository.Models
{
    public class Slot
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string SlotValue { get; set; }

    }
}
