using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
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
