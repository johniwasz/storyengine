using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public interface IOutboundMessageLogger
    {

        /// <summary>
        /// Validates and saves the notification request.
        /// </summary>
        /// <param name="smsRequest"></param>
        /// <returns></returns>
        Task<OutboundBatchRecord> ProcessNotificationRequestAsync(SmsNotificationRequest smsRequest);




        Task UpdateOutboundMessageBatchAsync(OutboundBatchRecord message);

    }
}
