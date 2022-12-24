using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Twitter
{
    public enum FollowEventType
    {
        [EnumMember(Value = "follow")]
        Follow = 1,
        [EnumMember(Value = "unfollow")]
        Unfollow = 2

    }


    public class FollowEvent
    {
        [JsonProperty(PropertyName = "type")]
        public FollowEventType Type { get; set; }

        [JsonConverter(typeof(TwitterMillisecondDateTimeConverter))]
        [JsonProperty(PropertyName = "created_timestamp")]
        public DateTime CreatedTimestamp { get; set; }

        [JsonProperty(PropertyName = "source")]
        public TwitterUser Source { get; set; }

        [JsonProperty(PropertyName = "target")]
        public TwitterUser Target { get; set; }
    }
}
