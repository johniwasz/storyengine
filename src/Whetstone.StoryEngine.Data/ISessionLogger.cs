using Whetstone.StoryEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Data
{
    public interface ISessionLogger
    {
        /// <summary>
        /// Saves the request and response for analysis.
        /// </summary>
        /// <param name="request">Request from the speech client.</param>
        /// <param name="response">Story Engine response which will be translated to the format of the calling client.</param>
        Task LogRequestAsync(StoryRequest request, StoryResponse response);


        Task LogRequestAsync(StoryRequest request, StoryResponse response, string rawClientRequestText, string rawClientResponseText);

        Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse);


        Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string rawClientRequestText, string rawClientResponseText);


        Task LogRequestAsync(RequestRecordMessage sessionQueueMsg);
    }
}
