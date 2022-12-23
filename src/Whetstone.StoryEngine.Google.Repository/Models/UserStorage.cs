using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Google.Repository.Models
{
    public class UserStorage
    {

        [JsonProperty(PropertyName = "userId")]
        public string UserId { get; set; }
    }
}
