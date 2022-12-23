using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Whetstone.StoryEngine.Google.Repository
{
    public interface IDialogFlowRepository
    {

        Task<APIGatewayProxyResponse> ProcessDialogFlowRequestAsync(APIGatewayProxyRequest dialogFlowRequest,
            ILambdaContext context);

    }
}
