using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.Google.Actions.V1;
using Whetstone.StoryEngine.Google.Actions.LambdaHost;

namespace Whetstone.StoryEngine.Google.WebApiHost.Controllers
{

    [Route("api/action")]
    [ApiController]
    public class ActionController : ControllerBase
    {



        // POST api/values
        [HttpPost]
        public async Task<ActionResult> PostAsync([FromBody][ModelBinder(typeof(HandlerBinder))] HandlerRequest value, [FromQuery] string alias, [FromQuery] string appid)
        {

            //Thread.Sleep(1000);
            //return new StatusCodeResult(500);
            //proxyRequest.Body = webhooktext;
            ILambdaContext lambdContext = new TestLambdaContext();


            ActionFunction actionFunc = new ActionFunction();

            APIGatewayProxyRequest proxyRequest = new APIGatewayProxyRequest
            {
                Body = value.ToJson(),
                Path = @"/",
                Headers = new Dictionary<string, string>
            {
                { "X-Forwarded-Proto", "https" }
            },
                HttpMethod = "POST"
            };

            if (!string.IsNullOrWhiteSpace(alias))
            {
                if (proxyRequest.QueryStringParameters == null)
                    proxyRequest.QueryStringParameters = new Dictionary<string, string>();
                proxyRequest.QueryStringParameters.Add("alias", alias);
            }

            if (!string.IsNullOrWhiteSpace(appid))
            {
                if (proxyRequest.QueryStringParameters == null)
                    proxyRequest.QueryStringParameters = new Dictionary<string, string>();
                proxyRequest.QueryStringParameters.Add("appid", appid);
            }

            APIGatewayProxyResponse resp = await actionFunc.FunctionHandlerAsync(proxyRequest, lambdContext);

            //HandlerResponse hookResp;
            //try
            //{
            //    hookResp =  HandlerResponse.FromJson(resp.Body);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //    throw;
            //}

            ContentResult contResult = new ContentResult
            {
                Content = resp.Body,
                StatusCode = 200,
                ContentType = "application/json"
            };


            return contResult;
        }
    }
}
