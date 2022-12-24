using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;

namespace Whetstone.StoryEngine.InboundSmsRepository
{
    public interface IInboundSmsProcessor
    {
        Task<INotificationRequest> ProcessInboundSmsMessageAsync(InboundSmsMessage msg);
    }
}
