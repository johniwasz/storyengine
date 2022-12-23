using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Auth.AccessControlPolicy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Whetstone.StoryEngine.ConfigUtilities.KeyPolicy
{
    public enum KeyGrantType
    {
        Encrypt =1,
        Decrypt =2,
        EncryptDecrypt=3
    }


    public class KeyPolicyRequest
    {
        [JsonProperty(PropertyName =  "ServiceToken")]
        public string ServiceToken { get; set; }

        /// <summary>
        /// ARN of the key with the policy to apply
        /// </summary>
        [JsonProperty(PropertyName = "Key")]
        public string Key { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "GrantType")]
        public KeyGrantType GrantType { get; set; }


        [JsonProperty(PropertyName = "RoleArn")]
        public string RoleArn { get; set; }

    }


}
