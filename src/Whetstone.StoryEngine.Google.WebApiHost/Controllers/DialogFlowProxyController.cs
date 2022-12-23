using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Google.WebApiHost;
using Whetstone.StoryEngine.Google.LambdaHost;
using FromBodyAttribute = Microsoft.AspNetCore.Mvc.FromBodyAttribute;
using HttpPostAttribute = Microsoft.AspNetCore.Mvc.HttpPostAttribute;
using HttpDeleteAttribute = Microsoft.AspNetCore.Mvc.HttpDeleteAttribute;
using HttpGetAttribute = Microsoft.AspNetCore.Mvc.HttpGetAttribute;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;
using HttpPutAttribute = Microsoft.AspNetCore.Mvc.HttpPutAttribute;
using System.Threading;

using System.Net.Http;
using System.Text;

namespace Whetstone.StoryEngine.Google.WebApiHost.Controllers
{
    [Route("api/dialogflow")]
    [ApiController]
    public class DialogFlowProxyController : ControllerBase
    {
        

        // POST api/values
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody] [ModelBinder(typeof(WebhookBinder))]
            WebhookRequest value, [FromQuery] string alias)
        {

            //Thread.Sleep(1000);
            //return new StatusCodeResult(500);

            APIGatewayProxyRequest proxyRequest = new APIGatewayProxyRequest();

            JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));
            proxyRequest.Body = value.ToString();
            //proxyRequest.Body = webhooktext;
            ILambdaContext lambdContext = new TestLambdaContext();
            NativeFunction dialogFunc = new NativeFunction();
            proxyRequest.Path = @"/";
            proxyRequest.Headers = new Dictionary<string, string>
            {
                { "X-Forwarded-Proto", "https" }
            };
            proxyRequest.HttpMethod = "POST";

            if (!string.IsNullOrWhiteSpace(alias))
            {
                proxyRequest.QueryStringParameters = new Dictionary<string, string>
                {
                    {"alias", alias}
                };

            }

            APIGatewayProxyResponse resp = await dialogFunc.FunctionHandlerAsync(proxyRequest, lambdContext);

            // object retObj = JsonConvert.DeserializeObject(resp.Body);

            WebhookResponse hookResp;
            try
            {
                hookResp = jsonParser.Parse<WebhookResponse>(resp.Body);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }



            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(hookResp.ToString(), Encoding.UTF8, "application/json");

            //var response = this.Request.CreateResponse(HttpStatusCode.OK);
            //response.Content = new StringContent(hookResp.ToString(), Encoding.UTF8, "application/json");

            ContentResult contResult = new ContentResult
            {
                Content = hookResp.ToString(),
                StatusCode = 200,
                ContentType = "application/json"
            };


            return contResult;
        }

    }
}
