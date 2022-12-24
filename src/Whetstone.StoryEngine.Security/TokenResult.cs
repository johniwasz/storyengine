using Newtonsoft.Json;
using System.Collections.Generic;

namespace Whetstone.StoryEngine.Security
{
    public class TokenResult
    {

        [JsonProperty(PropertyName = "authToken")]
        public string AuthToken { get; set; }

        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }

        [JsonProperty(PropertyName = "expiresIn")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "idToken")]
        public string IdToken { get; set; }

        [JsonProperty(PropertyName = "tokenType")]
        public string TokenType { get; set; }


        [JsonProperty(PropertyName = "permissions")]
        public IEnumerable<string> Permissions
        {
            get;
            set;
        }
    }
}
