using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository
{


    public interface ISmsHandler
    {
        Task<OutboundBatchRecord> SendOutboundSmsMessagesAsync(OutboundBatchRecord message);



    }
}
