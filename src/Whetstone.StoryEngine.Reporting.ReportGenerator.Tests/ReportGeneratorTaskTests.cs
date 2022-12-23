using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Model;
using Amazon.Lambda.TestUtilities;
using Amazon.StepFunctions;
using Amazon.StepFunctions.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Reporting.Models;
using Whetstone.StoryEngine.Reporting.ReportRepository;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Xunit;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class ReportGeneratorTaskTests
    {
        [Fact]
        public async Task SendReportRequest()
        {
            string bootstrapParam = "/storyengine/dev/bootstrap";
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, bootstrapParam);

            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");

            ReportRequest repRequest = new ReportRequest();
            repRequest.ReportName = "crxstateline";
            TestLambdaContext testContext = new TestLambdaContext();
            repRequest.Parameters = new Dictionary<string, dynamic>();


            repRequest.Parameters.Add("ptitleid", Guid.Parse("616a5f8e-fa08-41bf-b5a0-7a1ed4160db4"));
            repRequest.Parameters.Add("pstartTime", new DateTime(2019, 8, 10, 0, 0, 0, DateTimeKind.Utc));
            repRequest.Parameters.Add("pendTime", new DateTime(2019, 8, 30, 0, 0, 0, DateTimeKind.Utc));
            repRequest.DeliveryTime = DateTime.UtcNow.AddSeconds(120);
       
            string messageText = JsonConvert.SerializeObject(repRequest);


            EnvironmentConfig envConfig = new EnvironmentConfig
            {
                Region = RegionEndpoint.USEast1
            };
            IOptions<EnvironmentConfig> envOptions = Options.Create(envConfig);


            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {

                builder.AddConsole();
                builder.AddDebug();

            });

            ILogger<ParameterStoreReader> paramReaderlogger = loggerFactory.CreateLogger<ParameterStoreReader>();

            IParameterStoreReader paramReader = new ParameterStoreReader(envOptions, paramReaderlogger);

           var bootYamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            string bootText = await  paramReader.GetValueAsync(bootstrapParam);

            BootstrapConfig bootConfig = bootYamlDeser.Deserialize<BootstrapConfig>(bootText);


     
            ReportGeneratorConfig repConfig = new ReportGeneratorConfig();
            repConfig.ReportBucket = bootConfig.ReportBucket;
            repConfig.ReportStepFunctionArn = bootConfig.ReportStepFunction;

            IOptions<ReportGeneratorConfig> repOptions = Options.Create(repConfig);


            ILogger<ReportRequestReceiver> logger = loggerFactory.CreateLogger<ReportRequestReceiver>();

            IReportRequestReceiver repRequestReceiver = new ReportRequestReceiver(envOptions, repOptions, logger);


            await repRequestReceiver.SaveReportRequestAsync(repRequest);
        }


        [Fact]
        public async Task GenerateSampleReport()
        {
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");

            System.Environment.SetEnvironmentVariable(Bootstrapping.DBUSERTYPE, "AdminUser");


            var repTasks = new ReportGenerator.Tasks();

            ReportRequest repRequest = new ReportRequest();
            repRequest.ReportName = "crxstateline";
            TestLambdaContext testContext = new TestLambdaContext();
            repRequest.Parameters = new Dictionary<string, dynamic>
            {
                { "ptitleid", Guid.Parse("616a5f8e-fa08-41bf-b5a0-7a1ed4160db4") },
                { "pstartTime", new DateTime(2019, 8, 10, 0, 0, 0, DateTimeKind.Utc) },
                { "pendTime", new DateTime(2019, 8, 30, 0, 0, 0, DateTimeKind.Utc) }
            };
            repRequest.DeliveryTime = DateTime.UtcNow.AddHours(1);


            string reportRequestJson = JsonConvert.SerializeObject(repRequest, Formatting.Indented);

            ReportSendStatus repOutput =  await repTasks.GenerateReportAsync(repRequest, testContext);


            string jsonMsg = JsonConvert.SerializeObject(repOutput, Formatting.Indented);

            repOutput = JsonConvert.DeserializeObject<ReportSendStatus>(jsonMsg);

            await repTasks.SendReportAsync(repOutput, testContext);

        }

    }
}
