using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{

    /// <summary>
    /// This includes system wide security settings.
    /// </summary>
    public class SecurityConfig
    {

        /// <summary>
        /// Defaults to Cognito if not set.
        /// </summary>
        [JsonProperty(PropertyName = "authenticatorType")]
        [YamlMember(Alias = "authenticatorType", Order = 1)]
        public AuthenticatorType? AuthenticatorType { get; set; }


        [JsonProperty(PropertyName = "cognito")]
        [YamlMember(Alias = "cognito", Order = 2)]
        public CognitoConfig Cognito { get; set; }
    }
}
