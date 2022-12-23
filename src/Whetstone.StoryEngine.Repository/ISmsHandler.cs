using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Repository
{


    public interface ISmsHandler
    {
        Task<OutboundBatchRecord> SendOutboundSmsMessagesAsync(OutboundBatchRecord message);



    }
}
