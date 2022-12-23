using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Amazon.Lambda.AspNetCoreServer;
using Microsoft.Extensions.DependencyInjection;

namespace Whetstone.StoryEngine.CoreApi
{
    /// <summary>
    /// This class extends from APIGatewayProxyFunction which contains the method FunctionHandlerAsync which is the 
    /// actual Lambda function entry point. The Lambda handler field should be set to
    /// 
    /// Sbs.StoryEngine.CoreApi::Sbs.StoryEngine.CoreApi.LambdaEntryPoint::FunctionHandlerAsync
    /// </summary>
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {

        /// <summary>
        /// The builder has configuration, logging and Amazon API Gateway already configured. The startup class
        /// needs to be configured in this method using the UseStartup<>() method.
        /// </summary>
        /// <param name="builder"></param>
        protected override void Init(IWebHostBuilder builder)
        {
            // Added per suggestions from:
            // https://github.com/aws/aws-lambda-dotnet/issues/240
            //NB: Serverless WebAPI needs to have special config to serve binary file types: 
            //RegisterResponseContentEncodingForContentType("image/png", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("image/jpeg", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("image/gif", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("image/apng", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("image/webp", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("image/x-icon", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("application/zip", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("application/octet-stream", ResponseContentEncoding.Base64);
            RegisterResponseContentEncodingForContentType("audio/mpeg", ResponseContentEncoding.Base64);
            RegisterResponseContentEncodingForContentType("audio/wav", ResponseContentEncoding.Base64);
            //RegisterResponseContentEncodingForContentType("*/*", ResponseContentEncoding.Base64);

            builder
                .UseStartup<StartUp>();

        }

    }
}
