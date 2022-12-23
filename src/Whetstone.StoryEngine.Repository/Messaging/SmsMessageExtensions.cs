using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;


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
