using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Google.Repository
{
    public interface IDialogFlowRepository
    {

        Task<APIGatewayProxyResponse> ProcessDialogFlowRequestAsync(APIGatewayProxyRequest dialogFlowRequest,
            ILambdaContext context);

    }
}
