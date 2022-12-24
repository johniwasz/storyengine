using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.InboundSmsRepository;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;


namespace Whetstone.StoryEngine.InboundSmsHandler
{
    public class SmsTasks : ClientLambdaBase
    {

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public SmsTasks() : base()
        {
        }


        public SmsTasks(IServiceProvider serviceProv) : base()
        {
            this.Services = serviceProv;
        }

#pragma warning disable IDE0060 // Remove unused parameter
        public async Task<INotificationRequest> SmsHandlerTask(InboundSmsMessage smsMessaage, ILambdaContext context)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            ILogger<SmsTasks> tasksLogger = Services.GetService<ILogger<SmsTasks>>();

            Stopwatch preprocessTime = new Stopwatch();
            preprocessTime.Start();

            if (smsMessaage == null)
            {
                tasksLogger.LogError($"{nameof(smsMessaage)} is null or missing");
                return null;
            }

            IInboundSmsProcessor inboundProcessor = Services.GetRequiredService<IInboundSmsProcessor>();

            INotificationRequest notifRequest = await inboundProcessor.ProcessInboundSmsMessageAsync(smsMessaage);

            preprocessTime.Stop();

            return notifRequest;
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {

            services.AddTransient<IInboundSmsProcessor, InboundSmsProcessor>();

        }
    }
}
