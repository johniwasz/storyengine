using MartinCostello.Testing.AwsLambdaTestServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class SendReportTest
    {


        [Fact]
        public async Task SendReportMessageTest()
        {
            string requestText = File.ReadAllText(@"Messages/reprequest.json");




            // Arrange
            using (LambdaTestServer server = new LambdaTestServer())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(100)))
                {

                    await server.StartAsync(cancellationTokenSource.Token);


                    // string json = JsonConvert.SerializeObject(value);

                    string jsonResponse;
                    LambdaTestContext context = await server.EnqueueAsync(requestText);

                    using (var httpClient = server.CreateClient())
                    {

                        // Act
                        await Program.RunAsync(httpClient, cancellationTokenSource.Token).ContinueWith(async x =>
                        {
                            // Assert
                            Assert.True(context.Response.TryRead(out LambdaTestResponse response));
                            Assert.True(response.IsSuccessful);

                            jsonResponse = await response.ReadAsStringAsync();


                            // Assert.Equal(new[] { 3, 2, 1 }, actual);
                        });

                    }
                }
            }


        }
    }
}
