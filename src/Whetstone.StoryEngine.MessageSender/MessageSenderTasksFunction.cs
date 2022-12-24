using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Data.EntityFramework;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Whetstone.StoryEngine.Repository.Phone;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Whetstone.StoryEngine.Serialization.ShallowJsonSerializer))]
namespace Whetstone.StoryEngine.MessageSender
{
    public class MessageSenderTasksFunction : EngineLambdaBase
    {




        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public MessageSenderTasksFunction() : base()
        {
        }


        public MessageSenderTasksFunction(IServiceProvider servProv) : base()
        {
            this.Services = servProv ?? throw new ArgumentNullException(nameof(servProv));
        }


        public async Task<IOutboundNotificationRecord> DispatchMessageAsync(IOutboundNotificationRecord input, ILambdaContext context)
        {

            this.StartProcessingTime();
            if (input == null)
                throw new ArgumentNullException("input cannot be null");

            OutboundBatchRecord outMsg = input as OutboundBatchRecord;

            if (outMsg == null)
                throw new ArgumentException($"{nameof(input)} is not the expected type OutboundSmsBatchRecord");

            ILogger<MessageSenderTasksFunction> functionLogger = Services.GetService<ILogger<MessageSenderTasksFunction>>();


            try
            {
                // This should resolve to the direct sender.

                var funcHandler = Services.GetService<Func<SmsHandlerType, ISmsHandler>>();
                if (funcHandler == null)
                    throw new Exception("SmsHandler retriever function not found");


                ISmsHandler smsHandler = funcHandler(SmsHandlerType.DirectSender);
                if (smsHandler == null)
                    throw new Exception("No SmsDirectSendHandler configured");

                // Get the SMS function
                Func<SmsSenderType, ISmsSender> smsFunc = Services.GetService<Func<SmsSenderType, ISmsSender>>();
                if (smsFunc == null)
                    throw new Exception("No Sms Function handler found");


                if (!outMsg.SmsFromNumberId.HasValue)
                {


                    var phoneOptions = Services.GetService<IOptions<PhoneConfig>>();

                    if (phoneOptions == null)
                        throw new Exception("No PhoneOptions configured");

                    PhoneConfig envPhoneConfig = phoneOptions?.Value;

                    if (envPhoneConfig == null)
                        throw new Exception($"No PhoneOptions found");

                    // TODO Process source phone number.
                    // input.SmsFromNumber = envPhoneConfig.SourceSmsNumber;
                }

                outMsg = await smsHandler.SendOutboundSmsMessagesAsync(outMsg);

                IOutboundMessageLogger outboundMessageLogger = Services.GetService<IOutboundMessageLogger>();

                await outboundMessageLogger.UpdateOutboundMessageBatchAsync(outMsg);

                // Remove the PII data so it doesn't end up in the Cloudwatch log.
                outMsg.SentToPhone = null;
            }
            catch (Exception ex)
            {
                functionLogger.LogError(ErrorEvents.LambdaFunctionConfigError, ex, $"Error sending message {outMsg.Id}");
                LogEndtime();
                // Must rethrow so that the step function processes the exception in the event of a failure.
                throw;
            }

            LogEndtime();
            return outMsg;
        }




        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        ///
        /// To use this handler to respond to an AWS event, reference the appropriate package from 
        /// https://github.com/aws/aws-lambda-dotnet#events
        /// and change the string input parameter to the desired event type.
        /// </summary>
        /// <param name="batchRequest"></param>
        /// <param name="context"></param>
        /// <returns></returns>


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        {

            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            DatabaseConfig dbConfig = bootConfig.DatabaseSettings;

            DataBootstrapping.ConfigureDatabaseService(services, dbConfig);

            services.AddTransient<SmsConsentDatabaseRepository>();

            services.AddSingleton<Func<SmsConsentRepositoryType, ISmsConsentRepository>>(serviceProvider => consentRepKey =>
            {
                ISmsConsentRepository consentRep = null;
                switch (consentRepKey)
                {
                    case SmsConsentRepositoryType.Database:
                        consentRep = serviceProvider.GetService<SmsConsentDatabaseRepository>();
                        break;
                    case SmsConsentRepositoryType.DynamoDb:
                        consentRep = serviceProvider.GetService<SmsConsentDynamoDBRepository>();
                        break;
                    default:
                        throw new KeyNotFoundException(); // or maybe return null, up to you
                }

                return consentRep;
            });



            services.AddSingleton<IOutboundMessageLogger, OutboundMessageDatabaseLogger>();
            base.ConfigureServices(services, config);

        }


    }
}
