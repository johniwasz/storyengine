using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Whetstone.Alexa;

namespace Whetstone.StoryEngine.AlexaProcessor
{
    public interface IAlexaRequestProcessor
    {

        Task<AlexaResponse> ProcessAlexaLambdaRequestAsync(AlexaRequest request, ILambdaContext context);

    }
}
