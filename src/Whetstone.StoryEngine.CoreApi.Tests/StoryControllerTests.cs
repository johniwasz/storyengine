
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.CoreApi.Tests
{
    public class StoryControllerTests : CoreApiTestBase
    {
        [Fact]
        public async Task GetTestFile()
        {

            var lambdaFunction = new LambdaEntryPoint();

            // Return the content of the new s3 object foo.txt
            var requestStr = File.ReadAllText("./SampleRequests/StoryController-Get.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("text/plain", response.Headers["Content-Type"]);
            Assert.Equal("Hello World", response.Body);
        }



        [Fact]
        public async Task GetVersion()
        {

            var lambdaFunction = new LambdaEntryPoint();

            // Return the content of the new s3 object foo.txt
            var requestStr = File.ReadAllText("./SampleRequests/StoryController-Get-Version.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("text/plain", response.Headers["Content-Type"]);
            Assert.Equal("Hello World", response.Body);
        }

    }
}
