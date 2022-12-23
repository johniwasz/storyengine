using Amazon.RDS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Whetstone.StoryEngine.Models.Twitter;
using Xunit;

namespace Whetstone.StoryEngine.UnitTests.Twitter
{
    public class TwitterSerialization
    {

        [Fact]
        public void DeserializeTwitterUserTest()
        {

            string twitterUserText = File.ReadAllText("Twitter/twitterusersample.json");

            TwitterUser user = JsonConvert.DeserializeObject<TwitterUser>(twitterUserText);
        }


        [Fact]
        public void DeserializeTwitterFollowEventTest()
        {

            string twitterUnfollowEventText = File.ReadAllText("Twitter/twitterfollowevent.json");

            FollowEvent user = JsonConvert.DeserializeObject<FollowEvent>(twitterUnfollowEventText);
        }


        [Fact]
        public void DeserializeTwitterUnfollowEventTest()
        {

            string eventRawText = File.ReadAllText("Twitter/activityunfollow.json");

            ActivityEventsEnvelope events = JsonConvert.DeserializeObject<ActivityEventsEnvelope>(eventRawText);
        }
    }
}
