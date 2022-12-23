using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.InboundSmsHandler;
using Whetstone.StoryEngine.InboundSmsRepository;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.UnitTests;
using Xunit;

namespace Whetstone.StoryEngine.UnitTests
{
    public class ClientLambaBaseTest
    {
        [Fact]
        public void GetSampleLogger()
        {


            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;

            ClientLambdaProxy lambdaProxy = new ClientLambdaProxy();

            Assert.True(lambdaProxy.IsLoggerAvailable());

        }



    }
}
