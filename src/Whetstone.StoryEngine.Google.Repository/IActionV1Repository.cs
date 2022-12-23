using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Google.Repository
{
    public interface IActionV1Repository
    {

        Task<APIGatewayProxyResponse> ProcessActionV1RequestAsync(APIGatewayProxyRequest dialogFlowRequest,
           ILambdaContext context);
    }
}
