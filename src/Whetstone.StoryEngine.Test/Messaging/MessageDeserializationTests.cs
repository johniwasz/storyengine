using Newtonsoft.Json;
using System.IO;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Xunit;

namespace Whetstone.StoryEngine.Test.Messaging
{
    public class MessageDeserializationTests
    {
        [Fact]
        public void ReadPhoneRequest()
        {


            string notificationPayload = File.ReadAllText("Messages/smsresponsemessage.json");

            INotificationRequest smsPushRequest = JsonConvert.DeserializeObject<SmsNotificationRequest>(notificationPayload);



        }


    }
}
