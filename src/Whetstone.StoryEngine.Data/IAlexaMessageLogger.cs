using Whetstone.Alexa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Data
{
    public interface IAlexaMessageLogger
    {
        Task LogRequestAsync(AlexaRequest request);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response, Exception error);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response,string errorMessage);

    }
}
