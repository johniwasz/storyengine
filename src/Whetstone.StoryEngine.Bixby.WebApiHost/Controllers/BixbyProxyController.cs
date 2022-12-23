using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Bixby.WebApiHost;
using Whetstone.StoryEngine.Bixby.LambdaHost;
using Whetstone.StoryEngine.Bixby.Repository.Models;

namespace Whetstone.StoryEngine.Bixby.WebApiHost.Controllers
{
    [Route("api/bixby")]
    [ApiController]
    public class BixbyProxyController : ControllerBase
    {


        // POST api/values
        [HttpPost]
        public async Task<object> PostAsync([FromBody] [ModelBinder(typeof(WebhookBinder))] BixbyRequest_V1 value)
        {

            APIGatewayProxyRequest proxyRequest = new APIGatewayProxyRequest
            {
                Body = JsonConvert.SerializeObject(value)
            };
            //proxyRequest.Body = webhooktext;
            ILambdaContext lambdContext = new TestLambdaContext();
            NativeFunction bixbyFunc = new NativeFunction();
            proxyRequest.Path = @"/";
            proxyRequest.Headers = new Dictionary<string, string>
            {
                { "X-Forwarded-Proto", "https" }
            };
            proxyRequest.HttpMethod = "POST";
            APIGatewayProxyResponse resp = await bixbyFunc.FunctionHandlerAsync(proxyRequest, lambdContext);
            this.Response.ContentType = "application/json";

            object retObj = JsonConvert.DeserializeObject(resp.Body);

            //WebhookResponse hookResp = jsonParser.Parse<WebhookResponse>(resp.Body);

            return retObj;

        }

    }
}
