using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.Test;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class WhetstoneCanFulfillTests : TestServerFixture
    {

        [Fact]
        public async Task ProcessCanFulfillRequest()
        {
            string fulfillRequest = File.ReadAllText(@"RequestSamples/canfulfillsample.json");

            AlexaRequest req = null;

            try
            {
                //  var jsonSer = new Amazon.Lambda.Serialization.Json.JsonSerializer();
                //  var stream = new MemoryStream();
                //  var writer = new StreamWriter(stream);
                //  writer.Write(fulfillRequest);
                //  writer.Flush();

                //req =  jsonSer.Deserialize<AlexaRequest>(stream);
                req = JsonConvert.DeserializeObject<AlexaRequest>(fulfillRequest);


            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }


            AlexaFunctionBase funcBase = new AlexaFunction();

            TestLambdaContext testContext = new TestLambdaContext();


            req.Request.Timestamp = DateTime.UtcNow;
            AlexaResponse resp = await funcBase.FunctionHandlerAsync(req, testContext);



        }

    }
}
