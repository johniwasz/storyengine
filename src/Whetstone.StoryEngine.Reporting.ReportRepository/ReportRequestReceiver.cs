using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Reporting.Models;

namespace Whetstone.StoryEngine.Reporting.ReportRepository
{

    public interface IReportRequestReceiver
    {
        Task SaveReportRequestAsync(ReportRequest request);
    }


    public class ReportRequestReceiver : IReportRequestReceiver
    {
        private readonly RegionEndpoint _curRegion;
        private readonly ILogger<ReportRequestReceiver> _logger;
        private readonly string _reportGeneratorArn;

        public ReportRequestReceiver(IOptions<EnvironmentConfig> envConfig, IOptions<ReportGeneratorConfig> reportConfig, ILogger<ReportRequestReceiver> logger)
        {
            if (envConfig == null)
                throw new ArgumentNullException($"{nameof(envConfig)}");


            if(envConfig.Value == null)
                throw new ArgumentNullException($"{nameof(envConfig)}", "Value property cannot be null");

            _curRegion = envConfig.Value.Region ??
                         throw new ArgumentNullException($"{nameof(envConfig)}", "Region cannot be null or empty");

            if(reportConfig == null)
                throw new ArgumentNullException($"{nameof(reportConfig)}");

            if(reportConfig.Value == null)
                throw new ArgumentNullException($"{nameof(reportConfig)}", "Value cannot be null or empty");

            if(string.IsNullOrWhiteSpace(reportConfig.Value.ReportStepFunctionArn))
                throw new ArgumentNullException($"{nameof(reportConfig)}", "ReportStepFunctionArn cannot be null or empty");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");

            _reportGeneratorArn = reportConfig.Value.ReportStepFunctionArn;

        }

        public async Task SaveReportRequestAsync(ReportRequest request)
        {
            if (request == null)
                throw new ArgumentNullException($"{nameof(request)}");

            if (string.IsNullOrWhiteSpace(request.ReportName))
                throw new ArgumentException($"{nameof(request)}", "ReportName cannot be null or empty");


            if (request.DeliveryTime.HasValue)
            {
                // If the delivery time is after current time
                if(request.DeliveryTime.Value > DateTime.UtcNow)
                    request.IsScheduled = true;
            }

            try
            {
                string messageText = JsonConvert.SerializeObject(request);

                using (IAmazonStepFunctions stepFuncClient = new AmazonStepFunctionsClient(_curRegion))
                {
                    Stopwatch callTime = Stopwatch.StartNew();
                    StartExecutionRequest reportFuncReq = new StartExecutionRequest
                    {
                        StateMachineArn = _reportGeneratorArn,
                        Input = messageText
                    };

                    var funcResp = await stepFuncClient.StartExecutionAsync(reportFuncReq);
                    callTime.Stop();
                    string execArn = funcResp.ExecutionArn;
                    _logger.LogInformation($"Report request submitted {execArn}: {callTime.ElapsedMilliseconds}ms");

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Report request failed");
                throw;

            }

        }
    }
}
