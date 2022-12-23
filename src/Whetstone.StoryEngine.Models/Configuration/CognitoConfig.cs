using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class CognitoConfig 
    {

        [JsonProperty(PropertyName = "userPoolId")]
        [YamlMember(Alias = "userPoolId", Order = 1)]
        public string UserPoolId { get; set; }



        [JsonProperty(PropertyName = "userPoolClientId")]
        [YamlMember(Alias = "userPoolClientId", Order = 2)]
        public string UserPoolClientId { get; set; }


        [JsonProperty(PropertyName = "userPoolClientSecret")]
        [YamlMember(Alias = "userPoolClientSecret", Order = 3)]
        public string UserPoolClientSecret { get; set; }


        /// <summary>
        /// This is optional. If the user pool is in another region then the current environment, then specify it here.
        /// </summary>
        [JsonProperty(PropertyName = "userPoolRegion")]
        [YamlMember(Alias = "userPoolRegion", Order = 4)]
        public string UserPoolRegion { get; set; }


    }
}
