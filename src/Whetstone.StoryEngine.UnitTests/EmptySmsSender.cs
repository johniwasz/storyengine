using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Messaging;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.OutboutSmsSender;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Messaging;

namespace Whetstone.UnitTests
{
    public class EmptySmsSender : ISmsSender
    {
        public SmsSenderType ProviderName => SmsSenderType.Unassigned;

        public EmptySmsSender()
        {
        }

        public static void SetupUnitTestSmsDependencies( IServiceCollection servCol )
        {
            servCol.AddTransient<EmptySmsSender>();

            servCol.AddTransient<Func<SmsSenderType, ISmsSender>>(serviceProvider => key =>
            {
                return serviceProvider.GetService<EmptySmsSender>();
            });

            var mocker = new MockFactory();

            ISmsHandler smsHandlerMock =  mocker.GetSmsHandler(true);

            servCol.AddTransient<ISmsHandler>(x => smsHandlerMock);

          //  servCol.AddTransient<SmsDirectSendHandler>(x => MockFactory.GetDirectSendMock(true));

        //    SmsDirectSendHandler



        }

#pragma warning disable CS1998
        public async Task<OutboundMessageLogEntry> SendSmsMessageAsync( SmsMessageRequest messageRequest)
        {
#pragma warning restore CS1998
            OutboundMessageLogEntry result = new OutboundMessageLogEntry();


            result.SendStatus = MessageSendStatus.SentToDispatcher;
            result.LogTime = DateTime.UtcNow;
            result.IsException = false;
            result.HttpStatusCode = 200;

            return result;
        }

    }
}
