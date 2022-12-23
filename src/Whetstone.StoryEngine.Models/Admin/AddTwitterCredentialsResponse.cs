using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddTwitterCredentialsResponse
    {

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }

    }
}
