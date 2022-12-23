using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.AspNetCoreServer;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Whetstone.StoryEngine.DependencyInjection;

namespace Whetstone.StoryEngine.CoreApi
{
    public class Program
    {

        public static void Main(string[] args)
        {

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
            {
                // Uncomment these for dev.
                Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");
                Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");
                Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);

                // Uncomment these for prod.
                // Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/Prod/bootstrap");
                // Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");
                // Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USWest2.SystemName);

                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                var lambdaEntry = new LambdaEntryPoint();
                var functionHandler = (Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>>)(lambdaEntry.FunctionHandlerAsync);
                using var handlerWrapper = HandlerWrapper.GetHandlerWrapper(functionHandler, new JsonSerializer());
                using var bootstrap = new LambdaBootstrap(handlerWrapper);
                bootstrap.RunAsync().Wait();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {

                    webBuilder.UseStartup<StartUp>();
                });
    }
}
