using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public interface ITwilioStatusCallbackHandler
    {

        Task ProcessTwilioStatusCallbackAsync(TwilioStatusUpdateMessage statusUpdateMessage);

    }
}
