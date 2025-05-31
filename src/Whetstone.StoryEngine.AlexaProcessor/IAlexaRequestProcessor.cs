using System.Threading.Tasks;
using Whetstone.Alexa;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public interface IAlexaRequestProcessor
    {

        Task<AlexaResponse> ProcessAlexaRequestAsync(AlexaRequest request, string alias);


        Task<AlexaResponse> ProcessAlexaRequestAsync(AlexaRequest request);

    }
}
