using System;
using System.Threading.Tasks;
using Whetstone.Alexa;

namespace Whetstone.StoryEngine.Data
{
    public interface IAlexaMessageLogger
    {
        Task LogRequestAsync(AlexaRequest request);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response, Exception error);

        Task LogResponseAsync(AlexaRequest request, AlexaResponse response, string errorMessage);

    }
}
