using Whetstone.StoryEngine.Models.Messaging.Sms;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class SmsConfig
    {
        [YamlMember(Alias = "messageDelaySeconds")]
        public int MessageDelaySeconds { get; set; }

        [YamlMember(Alias = "messageSendRetryLimit")]
        public int MessageSendRetryLimit { get; set; }

        [YamlMember(Alias = "notificationStepFunctionArn")]
        public string NotificationStepFunctionArn { get; set; }

        [YamlMember(Alias = "engineSmsHandler")]
        public SmsHandlerType SmsHandlerType { get; set; }

        [YamlMember(Alias = "smsSenderType")]
        public SmsSenderType SmsSenderType { get; set; }

        [YamlMember(Alias = "sourceNumber")]
        public string SourceNumber { get; set; }


    }
}
