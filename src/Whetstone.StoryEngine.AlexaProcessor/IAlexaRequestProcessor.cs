using Amazon.Lambda.Core;
using System.Threading.Tasks;
using Whetstone.Alexa;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public interface IAlexaRequestProcessor
    {

        Task<AlexaResponse> ProcessAlexaLambdaRequestAsync(AlexaRequest request, ILambdaContext context);

    }
}
