using MartinCostello.Testing.AwsLambdaTestServer;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.LambdaUtilities.Tests
{
    public class NativeFunctionTests
    {


        [Fact]
        public async Task ProcessLambdaUtilityRequest()
        {

            string text = File.ReadAllText("Messages/ConfigUpdateRequest.json");

            // CustomResourceRequest resourceReq = JsonConvert.DeserializeObject<CustomResourceRequest>(text);



            // Arrange
            using (LambdaTestServer server = new LambdaTestServer())
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(100)))
                {

                    await server.StartAsync(cancellationTokenSource.Token);


                    // string json = JsonConvert.SerializeObject(value);

                    string jsonResponse;
                    LambdaTestContext context = await server.EnqueueAsync(text);

                    using (var httpClient = server.CreateClient())
                    {

                        // Act
                        await Program.RunAsync(httpClient, cancellationTokenSource.Token).ContinueWith(async x =>
                        {
                            // Assert
                            Assert.True(context.Response.TryRead(out LambdaTestResponse response));
                            Assert.True(response.IsSuccessful);

                            jsonResponse = await response.ReadAsStringAsync();

                            //    AlexaResponse actual = JsonConvert.DeserializeObject<AlexaResponse>(jsonResponse);

                            // Assert.Equal(new[] { 3, 2, 1 }, actual);
                        });

                    }
                }
            }
        }
    }
}
