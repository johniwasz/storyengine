using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.ReportRepository;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Whetstone.StoryEngine.Reporting.ReportGenerator
{
    public class Tasks : ReportTaskBase
    {

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Tasks() : base()
        {


        }


        public async Task<ReportSendStatus> GenerateReportAsync(ReportRequest repRequest, ILambdaContext context)
        {
            if (repRequest == null)
                throw new ArgumentNullException(nameof(repRequest));

            if (string.IsNullOrWhiteSpace(repRequest.ReportName))
                throw new ArgumentException($"{nameof(repRequest)} ReportName cannot be null or empty. Report name is required.");

            var reportProcessor = Services.GetRequiredService<IReportRequestProcessor>();

            ReportSendStatus repOutput = await reportProcessor.ProcessReportRequestAsync(repRequest);


            return repOutput;
        }

        public async Task<ReportSendStatus> SendReportAsync(ReportSendStatus repOutput, ILambdaContext context)
        {

            ILogger<Tasks> reportLogger = Services.GetService<ILogger<Tasks>>();

            if (repOutput == null)
                throw new ArgumentNullException(nameof(repOutput));


            if (repOutput.Destination == null)
                throw new ArgumentNullException(nameof(repOutput), "Destination property cannot be null. Destination is required.");

            // Figure out what type of destination it is so we know where to send it.

            var destinationType = repOutput.Destination.DestinationType;
            reportLogger.LogInformation($"Destination type is {destinationType}");


            var reportSenderFunc = Services.GetRequiredService<Func<ReportDestinationType, IReportSender>>();

            IReportSender sender = reportSenderFunc(destinationType);
            reportLogger.LogInformation($"Load the related report sender");


            ReportSendStatus sendStatus = await sender.SendReportsAsync(repOutput);


            return sendStatus;
        }


    }
}
