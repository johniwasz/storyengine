using Amazon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository.Amazon;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class SmsStepFunctionHandler : ISmsHandler
    {
        private readonly ILogger<SmsStepFunctionHandler> _logger;

        private SmsStepFunctionHandlerConfig _smsStepFunctionConfig;
        private RegionEndpoint _endpoint;
        private IStepFunctionSender _stepFunctionSender;

        public SmsStepFunctionHandler(IOptions<EnvironmentConfig> envConfig,
                                      IOptions<SmsStepFunctionHandlerConfig> config,
                                      IStepFunctionSender stepFunctionSender,
                                      ILogger<SmsStepFunctionHandler> logger)
        {


            _smsStepFunctionConfig = config?.Value ?? throw new ArgumentNullException($"{nameof(config)}");
            _endpoint = envConfig.Value?.Region ?? throw new ArgumentNullException($"{nameof(envConfig)}", "Region property cannot be null or empty");
            _stepFunctionSender = stepFunctionSender ?? throw new ArgumentNullException($"{nameof(stepFunctionSender)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(SmsStepFunctionHandler)}");
        }


        public async Task<OutboundBatchRecord> SendOutboundSmsMessagesAsync(OutboundBatchRecord message)
        {

            string smsFunctionEndpoint = _smsStepFunctionConfig.ResourceName;

            _logger.LogInformation($"Scheduling {message.Messages.Count} messages to number {message.SmsToNumberId} from number {message.SmsFromNumberId} for session {message.TitleUserId}");
            await _stepFunctionSender.SendMessageToStepFunctionAsync<OutboundBatchRecord>(smsFunctionEndpoint, message);
            return message;
        }




    }

}
