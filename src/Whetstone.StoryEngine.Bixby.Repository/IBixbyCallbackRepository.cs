using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

namespace Whetstone.StoryEngine.Bixby.Repository
{
    public interface IBixbyCallbackRepository
    {

        Task<APIGatewayProxyResponse> ProcessBixbyCallbackRequestAsync(APIGatewayProxyRequest BixbyCallbackRequest,
            ILambdaContext context);

    }
}
