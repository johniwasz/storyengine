using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public interface IMessageReporter
    {

        Task<List<MessageConsentReportRecord>> GetMessageConsentReportAsync(MessageConsentReportRequest reportRequest);

    }
}
