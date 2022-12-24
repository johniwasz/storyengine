using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using Whetstone.StoryEngine.DependencyInjection;

namespace Whetstone.StoryEngine.Google.WebApiHost
{
    public class Program
    {
        public static void Main(string[] args)
        {

            Environment.SetEnvironmentVariable(ClientLambdaBase.STORYBUCKETCONFIG, "whetstonebucket-dev-s3bucket-1nridm382p5vm");
            Environment.SetEnvironmentVariable(ClientLambdaBase.CACHESLIDINGCONFIG, "10000");
            Environment.SetEnvironmentVariable(ClientLambdaBase.CACHETABLECONFIG, "Whetstone-CacheTable-Dev-CacheTable-1A0X189QJZXYD");
            Environment.SetEnvironmentVariable(ClientLambdaBase.LOGLEVELCONFIG, "Debug");
            Environment.SetEnvironmentVariable(ClientLambdaBase.MESSAGESTEPFUNCTIONCONFIG, "arn:aws:states:us-east-1:940085449815:stateMachine:MessageSenderStateMachine-TatdcODo5DL1");
            Environment.SetEnvironmentVariable(ClientLambdaBase.SESSIONAUDITURLCONFIG, "https://sqs.us-east-1.amazonaws.com/940085449815/WhetstoneQueue-Dev-SessionAuditQueue-ZQNCFM1I9XSL");

            Environment.SetEnvironmentVariable(ClientLambdaBase.TWILIOLIVESECRETKEYCONFIG, "dev/twilio/live/Whetstone-MessageSender-Dev");

            Environment.SetEnvironmentVariable(ClientLambdaBase.TWILIOTESTSECRETKEYCONFIG, "dev/twilio/test/Whetstone-MessageSender-Dev");
            Environment.SetEnvironmentVariable(ClientLambdaBase.USERTABLECONFIG, "Whetstone-DynamoDb-Dev-UserTable-V0KS2V0Z2Y4J");


            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
