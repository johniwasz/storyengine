using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class TwitterWebhookListItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get; set;
        }

        [JsonProperty(PropertyName = "url")]
        public string Url
        {
            get; set;
        }

        [JsonProperty(PropertyName = "valid")]
        public bool Valid
        {
            get; set;
        }


        [JsonProperty(PropertyName = "createdAt")]
        public DateTimeOffset CreatedAt
        {
            get; set;
        }

    }
}
