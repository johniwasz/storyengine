using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.CoreApi.Models
{
    public class RefreshTokenRequest
    {

        [JsonProperty(PropertyName = "refreshToken")]
        public string RefreshToken { get; set; }
    }
}
