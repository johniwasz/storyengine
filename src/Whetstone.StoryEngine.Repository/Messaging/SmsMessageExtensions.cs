using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Messaging.Sms;


namespace Whetstone.StoryEngine.Repository.Messaging
{
    public static class SmsMessageExtensions
    {


        public static void AddSmsSenders(this IServiceCollection services)
        {

            services.AddTransient<SmsSnsSender>();
            services.AddTransient<SmsTwilioSender>();

            services.AddTransient<Func<SmsSenderType, ISmsSender>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case SmsSenderType.Sns:
                        return serviceProvider.GetService<SmsSnsSender>();
                    case SmsSenderType.Twilio:
                        return serviceProvider.GetService<SmsTwilioSender>();
                    default:
                        throw new KeyNotFoundException();
                }
            });



        }

    }
}
