using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AddVersionDeploymentRequest
    {



        [JsonProperty(PropertyName = "clientType", NullValueHandling = NullValueHandling.Ignore)]
        public Client ClientType { get; set; }

        [JsonProperty(PropertyName = "clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "alias", NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }


    }
}
