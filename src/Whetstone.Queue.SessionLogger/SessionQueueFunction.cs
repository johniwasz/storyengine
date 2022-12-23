using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Whetstone.Queue.SessionLogger.Repository;
using Whetstone.StoryEngine;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.Queue.SessionLogger
{
    public class SessionQueueFunction : SessionQueueFuncBase
    {

        /// <summary>
        /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
        /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
        /// region the Lambda function is executed in.
        /// </summary>
        public SessionQueueFunction() : base()
        {
      
        }


#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
        /// to respond to SQS messages.
        /// </summary>
        /// <param name="evnt"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task QueueFunctionHandler(SQSEvent evnt, ILambdaContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            ILogger<SessionQueueFunction> logger = Services.GetService<ILogger<SessionQueueFunction>>();

           Stopwatch queueTime = Stopwatch.StartNew();

            if (evnt == null)
            {
                logger.LogError("SQS Event is null");
                return;
            }

            if (evnt.Records == null)
            {
                logger.LogError("SQS Event records are null");
                return;
            }

            logger.LogInformation($"SQS Event record count: {evnt.Records.Count}");


            var queueSessionProcessor = Services.GetRequiredService<IQueueSessionProcessor>();

            await queueSessionProcessor.ProcessSessionLogMessages(evnt.Records);

            queueTime.Stop();

            LogEndtime(queueTime);
        }

    }
}
