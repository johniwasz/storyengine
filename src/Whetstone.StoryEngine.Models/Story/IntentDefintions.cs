using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Models.Story
{
    public class IntentDefintions
    {


        [JsonProperty("intents")]
        public List<Intent> Intents { get; set; }


        [JsonProperty("slots")]
        public List<SlotValue> Slots { get; set; }
    }
}
