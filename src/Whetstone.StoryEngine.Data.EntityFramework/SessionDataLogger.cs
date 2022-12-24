using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class SessionDataLogger : SessionLoggerBase, ISessionLogger
    {
        private readonly ILogger<SessionDataLogger> _dataLogger;

        private readonly IUserContextRetriever _contextRetriever;


        public SessionDataLogger(IUserContextRetriever contextRetriever, ILogger<SessionDataLogger> logger) : base()
        {
            _contextRetriever = contextRetriever ?? throw new ArgumentNullException($"{nameof(contextRetriever)}");
            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }


        public async Task LogRequestAsync(StoryRequest request, StoryResponse response)
        {

            await LogRequestAsync(request, response, null, null);

        }

        public async Task LogRequestAsync(RequestRecordMessage sessionQueueMsg)
        {
            if (sessionQueueMsg == null)
                throw new ArgumentNullException($"{nameof(sessionQueueMsg)} cannot be null");

            Stopwatch logtime = new Stopwatch();
            logtime.Start();

            try
            {
                DateTime? startTime = null;

                if (sessionQueueMsg.IsNewSession.GetValueOrDefault(false))
                    startTime = sessionQueueMsg.SelectionTime;


                using (var context = await _contextRetriever.GetUserDataContextAsync())
                {

                    await context.LogEngineRequestAsync(sessionQueueMsg.EngineUserId, sessionQueueMsg.EngineSessionId, sessionQueueMsg.EngineRequestId, sessionQueueMsg.SessionId, sessionQueueMsg.RequestId,
                        sessionQueueMsg.DeploymentId, sessionQueueMsg.UserId,
                        sessionQueueMsg.Locale, sessionQueueMsg.IntentName, sessionQueueMsg.Slots, sessionQueueMsg.ProcessDuration,
                        startTime, sessionQueueMsg.SelectionTime, sessionQueueMsg.RequestType, sessionQueueMsg.PreNodeActionLog,
                        sessionQueueMsg.MappedNode, sessionQueueMsg.PostNodeActionLog, sessionQueueMsg.CanFulfill, sessionQueueMsg.SlotFulFillment,
                        sessionQueueMsg.IntentConfidence, sessionQueueMsg.RawText, sessionQueueMsg.RequestAttributes, sessionQueueMsg.SessionAttributes, sessionQueueMsg.IsFirstSession,
                        sessionQueueMsg.RequestBodyText, sessionQueueMsg.ResponseBodyText, sessionQueueMsg.EngineErrorText, sessionQueueMsg.ResponseConversionErrorText, sessionQueueMsg.IsGuest);
                }



            }
            catch (PostgresException pgEx)
            {
                if (pgEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY, StringComparison.OrdinalIgnoreCase) && (pgEx.ConstraintName.Equals("PK_engine_requestaudit", StringComparison.OrdinalIgnoreCase)))
                {
                    // Duplicate entries will not result in an exception
                    Logger.LogInformation(pgEx, $"Request Id {sessionQueueMsg.EngineRequestId} with session {sessionQueueMsg.EngineSessionId} has already been added");
                }
                else
                {
                    Logger.LogError(pgEx, "SQL error adding session entry");
                    throw;
                }
            }
            logtime.Stop();

            _dataLogger.LogInformation($"Data log time for request {sessionQueueMsg.EngineRequestId} and response is {logtime.ElapsedMilliseconds}ms");


        }

        public async Task LogRequestAsync(StoryRequest request, StoryResponse response, string requestText, string responseText)
        {
            if (request == null)
                throw new ArgumentException($"{nameof(request)} cannot be null");


            AuditBehavior auditBehavior = response.AuditBehavior.GetValueOrDefault(AuditBehavior.RecordAll);

            if (!request.IsPingRequest.GetValueOrDefault(false) && auditBehavior != AuditBehavior.RecordNone)
            {
                if (response == null)
                    throw new ArgumentException($"{nameof(response)} cannot be null");

                if (request.SessionContext == null)
                    throw new ArgumentException($"{nameof(request)} SessionContext property cannot be null");

                if (!request.SessionContext.EngineSessionId.HasValue)
                    throw new ArgumentException($"{nameof(request)} SessionContext.EngineSessionId property cannot be null");

                Stopwatch logtime = new Stopwatch();
                logtime.Start();

                try
                {
                    DateTime? startTime = null;

                    if (request.IsNewSession.GetValueOrDefault(false))
                        startTime = request.RequestTime;

                    EngineSessionContext engineContext = request.SessionContext;
                    Guid? engineUserId = engineContext.EngineUserId;
                    Guid engineSessionId = engineContext.EngineSessionId.Value;
                    Guid engineRequestId = request.EngineRequestId;




                    if (auditBehavior == AuditBehavior.RecordEngineResponseOnly)
                    {
                        using (var context = await _contextRetriever.GetUserDataContextAsync())
                        {

                            await context.LogEngineRequestAsync(engineUserId, engineSessionId, engineRequestId, null, null, request.SessionContext.TitleVersion.DeploymentId.Value, null, null, null,
                              null, response.ProcessDuration, startTime, null, null, response.PreNodeActionLog, response.NodeName, response.PostNodeActionLog, null,
                              null, null, null, null, null, null, requestText, responseText, response.EngineErrorText, null, null);
                        }
                    }
                    else
                    {
                        using (var context = await _contextRetriever.GetUserDataContextAsync())
                        {
                            await context.LogEngineRequestAsync(engineUserId, engineSessionId, engineRequestId, request.SessionId, request.RequestId, request.SessionContext.TitleVersion.DeploymentId.Value, request.UserId, request.Locale, request.Intent,
                            request.Slots, response.ProcessDuration, startTime, request.RequestTime, request.RequestType, response.PreNodeActionLog, response.NodeName, response.PostNodeActionLog, null,
                            null, request.IntentConfidence, request.RawText, request.RequestAttributes, request.SessionAttributes, request.IsNewSession, requestText, responseText, response.EngineErrorText, null, request.IsGuest);
                        }


                    }

                }
                catch (PostgresException pgEx)
                {

                    if (pgEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY, StringComparison.OrdinalIgnoreCase) && pgEx.ConstraintName.Equals("PK_intent_action", StringComparison.OrdinalIgnoreCase))
                    {
                        // Duplicate entries will not result in an exception
                        Logger.LogWarning(pgEx, $"Request Id {request.RequestId} with session {request.SessionId} has already been added");
                    }
                    else
                    {
                        Logger.LogError(pgEx, "SQL error adding session entry");
                        throw;
                    }
                }
                logtime.Stop();

                _dataLogger.LogInformation($"Data log time for request {request.RequestId} and response is {logtime.ElapsedMilliseconds}ms");
            }
            else
                _dataLogger.LogDebug($"Not logging session direct to database for ping request {request.RequestId} or audit behavior {auditBehavior}");
        }

        protected override ILogger Logger
        {
            get
            {
                return _dataLogger;
            }
        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse)
        {
            await LogRequestAsync(request, fulfillResponse, null, null);

        }

        public async Task LogRequestAsync(StoryRequest request, CanFulfillResponse fulfillResponse, string requestText, string responseText)
        {
            if (request == null)
                throw new ArgumentException($"{nameof(request)} cannot be null");

            if (!request.IsPingRequest.GetValueOrDefault(false))
            {
                if (fulfillResponse == null)
                    throw new ArgumentException($"{nameof(fulfillResponse)} cannot be null");

                if (request.SessionContext == null)
                    throw new ArgumentException($"{nameof(request)} SessionContext property cannot be null");

                if (!request.SessionContext.EngineUserId.HasValue)
                    throw new ArgumentException($"{nameof(request)} SessionContext.EngineUserId property cannot be null");

                if (!request.SessionContext.EngineSessionId.HasValue)
                    throw new ArgumentException($"{nameof(request)} SessionContext.EngineSessionId property cannot be null");

                Stopwatch logtime = new Stopwatch();
                logtime.Start();

                try
                {

                    DateTime? startTime = null;

                    if (request.IsNewSession.GetValueOrDefault(false))
                        startTime = request.RequestTime;

                    EngineSessionContext engineContext = request.SessionContext;
                    Guid engineUserId = engineContext.EngineUserId.Value;
                    Guid engineSessionId = engineContext.EngineSessionId.Value;
                    Guid engineRequestId = request.EngineRequestId;

                    using (var context = await _contextRetriever.GetUserDataContextAsync())
                    {
                        await context.LogEngineRequestAsync(engineUserId, engineSessionId, engineRequestId, request.SessionId, request.RequestId, request.SessionContext.TitleVersion.DeploymentId.Value, request.UserId, request.Locale, request.Intent,
                      request.Slots, fulfillResponse.ProcessDuration, startTime, request.RequestTime, request.RequestType, null, null, null, fulfillResponse.CanFulfill,
                      fulfillResponse.SlotFulFillment, request.IntentConfidence, request.RawText, request.RequestAttributes, request.SessionAttributes, false, requestText, responseText,
                      fulfillResponse.EngineErrorText, null, request.IsGuest);
                    }

                }
                catch (PostgresException pgEx)
                {

                    if (pgEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY, StringComparison.OrdinalIgnoreCase))
                    {
                        // Duplicate entries will not result in an exception
                        Logger.LogWarning(pgEx, $"Request Id {request.RequestId} with session {request.SessionId} has already been added");
                    }
                    else
                    {
                        Logger.LogError(pgEx, "SQL error adding session entry");
                        throw;
                    }
                }
                _dataLogger.LogInformation($"Data log time for request {request.RequestId} and canfullintent is {logtime.ElapsedMilliseconds}ms");
            }
            else
                _dataLogger.LogDebug($"Not logging canfulfill request to database for ping request {request.RequestId}");
        }


    }
}
