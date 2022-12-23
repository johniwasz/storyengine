using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    // For more info on how to integrate a Web API with Cognito
    //https://developerhandbook.com/aws/how-to-use-aws-cognito-with-net-core/


    public class OpenIdSecurity
    {

        [JsonProperty(PropertyName = "clientId")]
        [YamlMember(Alias = "clientId")]
        public string ClientId { get; set; }


        [JsonProperty(PropertyName = "metadataAddress")]
        [YamlMember(Alias = "metadataAddress")]
        public string MetadataAddress { get; set; }

        [JsonProperty(PropertyName = "responseType")]
        [YamlMember(Alias = "responseType")]
        public string ResponseType { get; set; }
        
    }
}
