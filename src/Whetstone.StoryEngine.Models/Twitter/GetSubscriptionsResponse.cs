using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class GetSubscriptionsResponse
    {

        [JsonProperty(PropertyName = "applicationId")]
        public string ApplicationId { get; set; }


        [JsonProperty(PropertyName = "subscriptions")]
        public List<SubscriptionResponse> Subscriptions { get; set; }
    }
}
