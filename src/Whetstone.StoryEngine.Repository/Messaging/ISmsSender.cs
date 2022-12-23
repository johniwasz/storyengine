using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Repository.Messaging
{



    public interface ISmsSender
    {
        Task<OutboundMessageLogEntry> SendSmsMessageAsync(SmsMessageRequest messageRequest);

        SmsSenderType ProviderName { get; }
    }
}
