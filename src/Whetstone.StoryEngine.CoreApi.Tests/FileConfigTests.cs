using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.CoreApi.Tests
{
    public class FileConfigTests : CoreApiTestBase
    {


        [Fact]
        public async Task GetTestFile()
        {

            var lambdaFunction = new LambdaEntryPoint();

            // Return the content of the new s3 object foo.txt
            var requestStr = File.ReadAllText("./SampleRequests/S3ProxyController-GetByKey.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("text/plain", response.Headers["Content-Type"]);
            Assert.Equal("Hello World", response.Body);
        }


        [Fact(DisplayName = "Post Animal Farm PI Update")]
        public async Task PostTestFile()
        {
            var lambdaFunction = new LambdaEntryPoint();

            // Return the content of the new s3 object foo.txt
            var requestStr = File.ReadAllText("./SampleRequests/FileConfigController-Post.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);

            var storyTitleText = File.ReadAllText("./JsonSamples/animalfarmtestupdate.json");
            request.Body = storyTitleText;
            // request.Headers = new Dictionary<string, string>();
            // request.Headers.Add("Content-Type", "application/json");

            // string textRequest = JsonConvert.SerializeObject(request, Formatting.Indented);

            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);

        }


    }
}
