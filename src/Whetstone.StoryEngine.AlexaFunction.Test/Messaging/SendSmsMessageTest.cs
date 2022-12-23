using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Amazon;
using Microsoft.Extensions.Caching.Memory;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Whetstone.StoryEngine.Repository.Messaging;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.AlexaFunction.Test.Messaging
{

    [ExcludeFromCodeCoverage]
    public  class SendSmsMessageTest
    {

        private ISmsSender senderDelegate(SmsSenderType key)
        {

            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();


            });


            switch (key)
            {
                case SmsSenderType.Twilio:
                    var twilioMoqOptions = new Mock<IOptionsSnapshot<TwilioConfig>>();

                    twilioMoqOptions.Setup(x => x.Get(It.IsAny<string>()))
                        .Returns((string envKey) =>
                        {
                            TwilioConfig config = new TwilioConfig();
                            config.StatusCallbackUrl = "https://reeaf6p08d.execute-api.us-east-1.amazonaws.com/Prod/v1/smsmessagestatus";
                            config.LiveCredentials = "";
                            return config;
                        });


                    EnvironmentConfig envConfig = new EnvironmentConfig(RegionEndpoint.USEast1, string.Empty);
                    IOptions<EnvironmentConfig> envOpts = Options.Create(envConfig);



                    ILogger<SecretStoreReader> readerLogger = logFactory.CreateLogger<SecretStoreReader>();

                    ISecretStoreReader secreReader = new SecretStoreReader(envOpts, readerLogger);


                    MemoryCacheOptions memConfig = new MemoryCacheOptions();
                    IOptions<MemoryCacheOptions> memOpts = Options.Create(memConfig);

                    IMemoryCache memCache = new MemoryCache(memOpts);

                    ILogger<SmsTwilioSender> smsSender = logFactory.CreateLogger<SmsTwilioSender>();


                    return new SmsTwilioSender(twilioMoqOptions.Object, secreReader, memCache, smsSender);
                default:
                    throw new KeyNotFoundException();
            }
        }


        internal static OutboundBatchRecord GetSmsOutboundMessage()
        {
            OutboundBatchRecord outMessage = new OutboundBatchRecord
            {
                SmsToNumberId = Guid.NewGuid(),


                EngineRequestId = Guid.NewGuid(),

                Id = Guid.NewGuid(),


                Messages = new List<OutboundMessagePayload>()
            };

            OutboundMessagePayload firstMessage = new OutboundMessagePayload();

            firstMessage.Message = "This is a text message";

            firstMessage.Results = new List<OutboundMessageLogEntry>();

        

            outMessage.Messages.Add(firstMessage);

            OutboundMessagePayload secondMessage = new OutboundMessagePayload();

            secondMessage.Message = "This is the second text";

            secondMessage.Results = new List<OutboundMessageLogEntry>();

         
            outMessage.Messages.Add(secondMessage);


            return outMessage;
        }

    }

}

