using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddTwitterCredentialsResponse
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

    }
}
