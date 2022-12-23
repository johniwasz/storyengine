using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using Whetstone.StoryEngine.Models.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SQSEvents.SQSEvent;
using System.Diagnostics;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Data.EntityFramework;

namespace Whetstone.Queue.SessionLogger.Repository
{
    public class QueueSessionProcessor : IQueueSessionProcessor
    {

        private readonly ILogger<QueueSessionProcessor> _logger; // = StoryEngineLogFactory.CreateLogger<MessageProcessor>();

        private readonly ISessionLogger _sessionLogger;

   

        public QueueSessionProcessor(ISessionLogger sessionLogger, ILogger<QueueSessionProcessor> logger)
        {
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");        

           _sessionLogger = sessionLogger ?? throw new ArgumentNullException($"{nameof(sessionLogger)}");

        }

        

        public async Task ProcessSessionLogMessages(List<SQSMessage> sqsMessages)
        {
            

            List<Exception> messageExceptions = new List<Exception>();

            if ((sqsMessages?.Any()).GetValueOrDefault(false))
            {
                _logger.LogInformation($"Processing {sqsMessages.Count} messages");

                foreach (SQSMessage sqsMessage in sqsMessages)
                {

                    Stopwatch messageProcessingTime = new Stopwatch();
                    messageProcessingTime.Start();

                    RequestRecordMessage sessionQueueMsg = null;
                    string messageBody = sqsMessage.Body;
                    _logger.LogInformation($"Processing message {sqsMessage.MessageId}");
                    try
                    {
                        sessionQueueMsg = JsonConvert.DeserializeObject<RequestRecordMessage>(messageBody);
                    }
                    catch (Exception ex)
                    {

                        messageExceptions.Add(new Exception(
                            $"Error deserializing message id {sqsMessage.MessageId} with receipt handle {sqsMessage.ReceiptHandle}: {messageBody}", ex));

                    }


                    if (sessionQueueMsg != null)
                    {


                        try
                        {

                            _logger.LogInformation($"Logging message id {sqsMessage.MessageId}  session id {sessionQueueMsg.EngineSessionId} request id {sessionQueueMsg.EngineRequestId} request and response");

                            await _sessionLogger.LogRequestAsync(sessionQueueMsg);
                        }
                        catch (PostgresException pgEx)
                        {
                            if (pgEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY, StringComparison.OrdinalIgnoreCase) && pgEx.ConstraintName.Equals("PK_intent_action", StringComparison.OrdinalIgnoreCase))
                            {
                                // Duplicate entries will not result in an exception
                                _logger.LogWarning(pgEx, $"Session Id {sessionQueueMsg.SessionId} with request id {sessionQueueMsg.EngineRequestId} and message id {sqsMessage.MessageId} has already been added: {messageBody}");
                            }
                            else
                            {
                                _logger.LogError(pgEx, $"SQL error adding session entry for session id {sessionQueueMsg.EngineSessionId} request id {sessionQueueMsg.EngineRequestId} and message id {sqsMessage.MessageId}: {messageBody}");
                                throw;
                            }
                        }
                        catch (Exception ex)
                        {
                            messageExceptions.Add(new Exception(
                                    $"Error logging session id {sessionQueueMsg.EngineSessionId} with request id {sessionQueueMsg.EngineRequestId} in message id {sqsMessage.MessageId} with receipt handle {sqsMessage.ReceiptHandle}: {messageBody}", ex));
                        }

                    }



                    messageProcessingTime.Stop();

                    _logger.LogInformation($"Total processing time for message {sqsMessage.MessageId}: {messageProcessingTime.ElapsedMilliseconds}");
                }
            }

            if (messageExceptions.Any())
            {
                if (messageExceptions.Count == 1)
                    throw new Exception("Single exception found when processing messages", messageExceptions.First());
                else
                    throw new AggregateException(messageExceptions);
            }
        }

    }
}
