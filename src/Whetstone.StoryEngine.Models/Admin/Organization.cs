using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class Organization
    {


        [JsonProperty(PropertyName = "id")]
        public Guid? Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "subscriptionLevel")]
        public string SubscriptionLevel { get; set; }


        [JsonProperty(PropertyName = "isenabled")]
        public bool IsEnabled { get; set; }

    }
}
