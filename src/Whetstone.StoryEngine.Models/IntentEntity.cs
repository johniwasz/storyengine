using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Whetstone.StoryEngine.Models
{

    /// <summary>
    /// Returned from the natural language processor.
    /// </summary>
    public class IntentEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
