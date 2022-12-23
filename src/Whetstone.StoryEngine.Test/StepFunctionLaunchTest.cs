using Amazon;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Xunit;

namespace Whetstone.StoryEngine.Test
{

    public class StepFunctionLaunchTest
    {

        [Fact]
        public void ActualSchedulingEngineStepFunctionCallTest()
        {
            var amazonStepFunctionsConfig = new AmazonStepFunctionsConfig { RegionEndpoint = RegionEndpoint.USEast1 };




            using (var amazonStepFunctionsClient = new AmazonStepFunctionsClient(amazonStepFunctionsConfig))
            {


                int total = 2;
                List<OutboundBatchRecord> outMessages = new List<OutboundBatchRecord>();

                for (int i = 0; i < total; i++)
                {
                    OutboundBatchRecord newMessage = GetSmsOutboundMessage();

                    newMessage.SmsToNumberId = Guid.NewGuid();
                    newMessage.SmsFromNumberId = Guid.NewGuid();
                    //newMessage.SmsToNumber = "+12675551212";
                    //if (i == 0)
                    //{
                    //    newMessage.SmsToNumber = "+12675551212";
                    //}
                    //else if (i==1)
                    //{
                    //    newMessage.SmsToNumber = "+16105551212";

                    //}
                   // newMessage.SmsFromNumber = "+12157099492";

                    //  newMessage.SmsFromNumber = null;

                   // newMessage.SmsApplicationId = "256060e139054c6e8402b44506b0d9f5";

                    newMessage.Messages = new List<OutboundMessagePayload>();

                    newMessage.Messages.Add(new OutboundMessagePayload
                    {
                        Message = $"First message counter {i} message id: {newMessage.Id} from Sns"
                    });

                    newMessage.Messages.Add(new OutboundMessagePayload
                    {
                        Message = $"Second message counter {i} message id: {newMessage.Id} from Sns"
                    });

                    outMessages.Add(newMessage);
                }


                var getCustomerBlock = new TransformBlock<OutboundBatchRecord, StartExecutionResponse>(
                    async msg =>
                    {
                        var jsonData1 = JsonConvert.SerializeObject(msg);
                        var startExecutionRequest = new StartExecutionRequest
                        {
                            Input = jsonData1,
                            Name = msg.Id.ToString(),
                            StateMachineArn = "arn:aws:states:us-east-1:940085449815:stateMachine:SaveMessageTestFunctions"
                        };
                        var taskStartExecutionResponse = await amazonStepFunctionsClient.StartExecutionAsync(startExecutionRequest);

                        return taskStartExecutionResponse;
                    }, new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded
                    });


                var writeCustomerBlock = new ActionBlock<StartExecutionResponse>(c => Console.WriteLine(c.HttpStatusCode));
                getCustomerBlock.LinkTo(
                    writeCustomerBlock, new DataflowLinkOptions
                    {
                        PropagateCompletion = true
                    });



                foreach (var id in outMessages)
                    getCustomerBlock.Post(id);

                getCustomerBlock.Complete();


                writeCustomerBlock.Completion.Wait();

            }
        }


        internal static OutboundBatchRecord GetSmsOutboundMessage()
        {
            return GetSmsOutboundMessage( Guid.NewGuid());
;
        }

        internal static OutboundBatchRecord GetSmsOutboundMessage(Guid messageId)
        {
            OutboundBatchRecord outMessage = new OutboundBatchRecord();


            outMessage.SmsFromNumberId = Guid.NewGuid();

            outMessage.SmsToNumberId = Guid.NewGuid();

            outMessage.EngineRequestId = Guid.NewGuid();

            outMessage.Id = messageId;

            outMessage.IsSaved = false;

            outMessage.Messages = new List<OutboundMessagePayload>();

            OutboundMessagePayload firstMessage = new OutboundMessagePayload();

            firstMessage.Message = "Here's a message sent from step functions";

            firstMessage.Results = new List<OutboundMessageLogEntry>();

            OutboundMessageLogEntry msgResult = new OutboundMessageLogEntry();
            msgResult.ExtendedStatus = "TXTSTAT";
            msgResult.HttpStatusCode = 200;
            msgResult.IsException = false;
            msgResult.LogTime= DateTime.UtcNow;

            firstMessage.Results.Add(msgResult);

            outMessage.Messages.Add(firstMessage);

            OutboundMessagePayload secondMessage = new OutboundMessagePayload();

            secondMessage.Message = "Here's another sent from step functions";

            secondMessage.Results = new List<OutboundMessageLogEntry>();

            OutboundMessageLogEntry badResult = new OutboundMessageLogEntry();
            
            badResult.ExtendedStatus = "ERR";
            badResult.HttpStatusCode = 500;
            badResult.IsException = true;
            badResult.LogTime = DateTime.UtcNow;
            secondMessage.Results.Add(badResult);


            OutboundMessageLogEntry goodResult = new OutboundMessageLogEntry();
            goodResult.IsException = false;
            goodResult.HttpStatusCode = 200;
            goodResult.LogTime = DateTime.UtcNow;
            goodResult.ExtendedStatus = "SUCCESS";
            secondMessage.Results.Add(goodResult);

            outMessage.Messages.Add(secondMessage);


            return outMessage;
        }
    }
}
