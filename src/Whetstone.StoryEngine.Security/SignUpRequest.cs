using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Security
{
    public class SignUpRequest
    {

        [JsonProperty(PropertyName = "name")]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string UserSecret { get; set; }


        [JsonProperty(PropertyName = "acceptedTerms")]
        public bool AreTermsAccepted { get; set; }

    }
}
