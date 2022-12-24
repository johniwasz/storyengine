using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Threading.Tasks;

namespace Whetstone.StoryEngine.Bixby.Repository
{
    public interface IBixbyCallbackRepository
    {

        Task<APIGatewayProxyResponse> ProcessBixbyCallbackRequestAsync(APIGatewayProxyRequest BixbyCallbackRequest,
            ILambdaContext context);

    }
}
