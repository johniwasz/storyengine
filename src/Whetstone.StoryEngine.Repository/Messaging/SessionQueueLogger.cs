using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository.Messaging
{
    public class SessionQueueLogger : ISessionLogger
    {

        private readonly ILogger<SessionQueueLogger> _logger;

        private SessionAuditConfig _auditConfig;
        private IWhetstoneQueue _queueSender;

        public SessionQueueLogger(
                        IOptions<SessionAuditConfig> sessionAuditConfig,
                        IWhetstoneQueue queueSender,
                        ILogger<SessionQueueLogger> logger)
        {
            _queueSender = queueSender ?? throw new ArgumentNullException($"{nameof(queueSender)} cannot be null");
            _auditConfig = sessionAuditConfig?.Value ?? throw new ArgumentNullException($"{nameof(sessionAuditConfig)} cannot be null");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }


        public async Task LogRequestAsync(StoryRequest request, StoryResponse response)
        {

            await LogRequestAsync(request, response, null, null);
        }

        public async Task LogRequestAsync(StoryRequest request, StoryResponse response, string requestText, string responseText)
        {
            if (!request.IsPingRequest.GetValueOrDefault(false))
            {

                AuditBehavior auditBehavior = response.AuditBehavior.GetValueOrDefault(AuditBehavior.RecordAll);

                if (auditBehavior != AuditBehavior.RecordNone)
                {

                    RequestRecordMessage queueMessage = new RequestRecordMessage();

                    // If the audit behavior blocks the recording of customer responses,
                    // don't record the inbound request.
                    if (auditBehavior != AuditBehavior.RecordEngineResponseOnly)
                        AssignRequestProperties(request, queueMessage);

                    AssignResponseProperties(response, queueMessage);

                    if (!string.IsNullOrWhiteSpace(requestText))
                        queueMessage.RequestBodyText = requestText;

                    if (!string.IsNullOrWhiteSpace(responseText))
                        queueMessage.ResponseBodyText = responseText;

                    await LogRequestAsync(queueMessage);
                }
            }
            else
                _logger.LogDebug($"Not logging session to queue for request {request.RequestId}");
        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse)
        {


            await LogRequestAsync(request, fulfillResponse, null, null);
        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string requestText, string responseText)
        {

            if (!request.IsPingRequest.GetValueOrDefault(false))
            {
                RequestRecordMessage queueMessage = new RequestRecordMessage();
                AssignRequestProperties(request, queueMessage);
                AssignCanfulfillProperties(fulfillResponse, queueMessage);

                if (!string.IsNullOrWhiteSpace(requestText))
                    queueMessage.RequestBodyText = requestText;

                if (!string.IsNullOrWhiteSpace(responseText))
                    queueMessage.ResponseBodyText = responseText;

                string queueUrl = _auditConfig.SessionAuditQueue;

                await _queueSender.AddMessageToQueueAsync<RequestRecordMessage>(queueUrl, queueMessage);
            }
            else
                _logger.LogDebug($"Not logging ping request to queue for request {request.RequestId}");
        }

        private void AssignRequestProperties(StoryRequest req, RequestRecordMessage queueMessage)
        {

            queueMessage.EngineUserId = req.SessionContext.EngineUserId;

            if (req.SessionContext == null)
            {
                _logger.LogError("Session context should not be null");
                queueMessage.EngineSessionId = default(Guid);
                queueMessage.DeploymentId = default(Guid);
            }
            else
            {
                queueMessage.EngineSessionId = (req.SessionContext?.EngineSessionId).GetValueOrDefault();
                queueMessage.DeploymentId = (req.SessionContext?.TitleVersion?.DeploymentId).GetValueOrDefault();
            }


            queueMessage.EngineRequestId = req.EngineRequestId;
            queueMessage.RequestId = req.RequestId;
            queueMessage.Locale = req.Locale;
            queueMessage.SelectionTime = req.RequestTime;
            queueMessage.SessionId = req.SessionId;
            queueMessage.IsFirstSession = req.IsNewSession;
            queueMessage.IsGuest = req.IsGuest;

            queueMessage.RequestType = req.RequestType;
            queueMessage.UserId = req.UserId;


            if (req.Slots != null && req.Slots.Keys.Any())
            {
                queueMessage.Slots = new Dictionary<string, string>();

                foreach (string reqSlotName in req.Slots.Keys)
                {
                    string slotVal = null;
                    // This ensures the user's phone number is not part of the audit log.
                    if (reqSlotName.Equals("phonenumber", StringComparison.OrdinalIgnoreCase))
                    {
                        slotVal = "REDACTED";
                    }
                    else
                    {
                        slotVal = req.Slots[reqSlotName];
                    }

                    queueMessage.Slots.Add(reqSlotName, slotVal);

                }
            }

            queueMessage.IntentName = req.Intent;
            queueMessage.IsNewSession = req.IsNewSession;
            queueMessage.RawText = req.RawText;
            queueMessage.RequestAttributes = req.RequestAttributes;
            queueMessage.IntentConfidence = req.IntentConfidence;

            if (queueMessage.IsNewSession.GetValueOrDefault(false))
            {
                queueMessage.SessionAttributes = req.SessionAttributes;
            }
        }

        private void AssignResponseProperties(StoryResponse resp, RequestRecordMessage queueMessage)
        {

            if (resp.SessionContext == null)
            {
                _logger.LogError("Session context should not be null");
                queueMessage.DeploymentId = default(Guid);
            }
            else
            {
                queueMessage.DeploymentId = (resp.SessionContext?.TitleVersion?.DeploymentId).GetValueOrDefault();
            }

            queueMessage.MappedNode = resp.NodeName;
            queueMessage.ProcessDuration = resp.ProcessDuration;
            queueMessage.PreNodeActionLog = resp.PreNodeActionLog;
            queueMessage.PostNodeActionLog = resp.PostNodeActionLog;
            queueMessage.EngineErrorText = resp.EngineErrorText;
            queueMessage.ResponseConversionErrorText = resp.ResponseConversionError;


        }

        private void AssignCanfulfillProperties(CanFulfillResponse fulfillResp, RequestRecordMessage queueMessage)
        {

            queueMessage.ProcessDuration = fulfillResp.ProcessDuration;
            queueMessage.SlotFulFillment = fulfillResp.SlotFulFillment;
            queueMessage.CanFulfill = fulfillResp.CanFulfill;
            queueMessage.EngineErrorText = fulfillResp.EngineErrorText;
            queueMessage.ResponseConversionErrorText = fulfillResp.ResponseConversionError;

        }

        public async Task LogRequestAsync(RequestRecordMessage sessionQueueMsg)
        {
            if (sessionQueueMsg == null)
                throw new ArgumentNullException($"{nameof(sessionQueueMsg)} cannot be null or empty");


            string queueUrl = _auditConfig.SessionAuditQueue;

            await _queueSender.AddMessageToQueueAsync<RequestRecordMessage>(queueUrl, sessionQueueMsg);



        }
    }
}
