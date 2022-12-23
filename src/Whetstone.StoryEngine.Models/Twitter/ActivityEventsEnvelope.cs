using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public class ActivityEventsEnvelope
    {


        [JsonProperty(PropertyName = "for_user_id")]
        public string ForUserId { get; set; }

        [JsonProperty(PropertyName = "follow_events")]
        public List<FollowEvent> FollowEvents { get; set; }
    }
}
