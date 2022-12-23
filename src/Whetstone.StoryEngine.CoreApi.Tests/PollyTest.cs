using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Xunit;

namespace Whetstone.StoryEngine.CoreApi.Tests
{
   
    public class PollyTest : CoreApiTestBase
    {
        [Fact]
        public async Task GetMp3()
        { 
            var lambdaFunction = new LambdaEntryPoint();

            // Return the content of the new s3 object foo.txt
            var requestStr = File.ReadAllText("./SampleRequests/PollyController-Post.json");
            var request = JsonConvert.DeserializeObject<APIGatewayProxyRequest>(requestStr);
            var context = new TestLambdaContext();
            var response = await lambdaFunction.FunctionHandlerAsync(request, context);

            Assert.Equal(200, response.StatusCode);
            Assert.Equal("text/plain", response.Headers["Content-Type"]);
            Assert.Equal("Hello World", response.Body);
        }


    }
}
