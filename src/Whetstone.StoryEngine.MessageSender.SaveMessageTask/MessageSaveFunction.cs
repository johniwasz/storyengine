using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Whetstone.StoryEngine.Data.DependencyInjection;
using Whetstone.StoryEngine.Repository.Phone;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;
using Whetstone.StoryEngine.Data.EntityFramework;

namespace Whetstone.StoryEngine.MessageSender.SaveMessageTask
{
    public class MessageSaveFunction :  EngineLambdaBase
    {


        private static Lazy<MessageSaveFunction> _nativeFunction = new Lazy<MessageSaveFunction>(GetNativeFunction);


        private static MessageSaveFunction GetNativeFunction()
        {
            return new MessageSaveFunction();
        }

        public MessageSaveFunction()
        {

        }

        public MessageSaveFunction(IServiceProvider servProv)
        {
            this.Services = servProv;

        }

        /// <summary>
        /// The main entry point for the custom runtime.
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main()
            => await RunAsync();

        public static async Task RunAsync(
            HttpClient httpClient = null,
            CancellationToken cancellationToken = default)
        {

            Amazon.Lambda.Serialization.Json.JsonSerializer ser =
                new Amazon.Lambda.Serialization.Json.JsonSerializer(x => x.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);


            using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper((Func<INotificationRequest, ILambdaContext, Task<IOutboundNotificationRecord>>)ProcessRequestAsync, ser))
            {
                // Instantiate a LambdaBootstrap and run it.
                // It will wait for invocations from AWS Lambda and call
                // the handler function for each one.
                using (var bootstrap = new LambdaBootstrap(httpClient ?? new HttpClient(), handlerWrapper))
                {
                    await bootstrap.RunAsync(cancellationToken);
                }
            }
        }


        public static async Task<IOutboundNotificationRecord> ProcessRequestAsync(INotificationRequest batchRequest, ILambdaContext context)
        {

            return await _nativeFunction.Value.SaveNotificationRequest(batchRequest, context);

        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        ///
        /// To use this handler to respond to an AWS event, reference the appropriate package from 
        /// https://github.com/aws/aws-lambda-dotnet#events
        /// and change the string input parameter to the desired event type.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>


        public async Task<IOutboundNotificationRecord> SaveNotificationRequest(INotificationRequest batchRequest, ILambdaContext context)
        {
            Stopwatch saveTime = new Stopwatch();
            saveTime.Start();

            ILogger<MessageSaveFunction> logger = Services.GetService<ILogger<MessageSaveFunction>>();

            IOutboundNotificationRecord outNotifRecord = null;
            try
            {
                if (batchRequest is SmsNotificationRequest notifReq)
                {
                    IOutboundMessageLogger outboundLogger = Services.GetRequiredService<IOutboundMessageLogger>();
                    OutboundBatchRecord outRecord = await outboundLogger.ProcessNotificationRequestAsync(notifReq);

                    // Remove any phone records from the output
                    outRecord.SentToPhone = null;

                    if (outRecord.Consent != null)
                        outRecord.Consent.Phone = null;

                    outNotifRecord = outRecord;
                }

            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    $"Unexpected exception when trying to save record {batchRequest.Id.GetValueOrDefault()}");
                throw;
            }
            finally
            {
                saveTime.Stop();
                logger.LogInformation($"Time to process notification message id {batchRequest.Id.GetValueOrDefault()}: {saveTime.ElapsedMilliseconds}ms");

            }


            return outNotifRecord;
        }


        private async Task<IOutboundNotificationRecord> SaveMessageUpdate(OutboundBatchRecord input, ILambdaContext context)
        {

            this.StartProcessingTime();
            if (input == null)
                throw new ArgumentNullException($"{nameof(input)}");

            OutboundBatchRecord outMsg = input as OutboundBatchRecord;

            if (outMsg == null)
                throw new ArgumentException($"{nameof(input)} is not the expected type OutboundSmsBatchRecord");

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
                        throw new Exception("No PhoneOptions found");

                }

                outMsg = await smsHandler.SendOutboundSmsMessagesAsync(outMsg);

                IOutboundMessageLogger outboundMessageLogger = Services.GetService<IOutboundMessageLogger>();

                await outboundMessageLogger.UpdateOutboundMessageBatchAsync(outMsg);

                // Remove the PII data so it doesn't end up in the Cloudwatch log.
                outMsg.SentToPhone = null;
            }
            catch (Exception ex)
            {
                ILogger<MessageSaveFunction> logger = Services.GetService<ILogger<MessageSaveFunction>>();

                logger.LogError(ErrorEvents.LambdaFunctionConfigError, ex, $"Error sending message {outMsg.Id}");
                LogEndtime();
                // Must rethrow so that the step function processes the exception in the event of a failure.
                throw;
            }

            LogEndtime();
            return outMsg;
        }


        protected override void ConfigureServices(IServiceCollection services, IConfiguration config)
        {

            BootstrapConfig bootConfig = Configuration.Get<BootstrapConfig>();

            DatabaseConfig dbConfig = bootConfig.DatabaseSettings;

            DataBootstrapping.ConfigureDatabaseService(services, dbConfig);

            services.AddTransient<SmsConsentDatabaseRepository>();

            services.AddSingleton<Func<SmsConsentRepositoryType, ISmsConsentRepository>>(serviceProvider => key =>
            {
                ISmsConsentRepository consentRep = default;
                switch (key)
                {
                    case SmsConsentRepositoryType.Database:
                        consentRep = serviceProvider.GetService<SmsConsentDatabaseRepository>();
                        break;
                    case SmsConsentRepositoryType.DynamoDb:
                        consentRep = serviceProvider.GetService<SmsConsentDynamoDBRepository>();
                        break;
                }

                return consentRep;
            });

            services.AddSingleton<IOutboundMessageLogger, OutboundMessageDatabaseLogger>();
            base.ConfigureServices(services, config);

        }

    }
}
