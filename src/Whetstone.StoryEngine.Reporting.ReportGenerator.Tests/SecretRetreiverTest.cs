using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using Xunit;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class SecretRetreiverTest
    {

        [Fact]
        public async Task GetSftpSecretAsyncTest()
        {



            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.AddDebug();


            });

            EnvironmentConfig envConfig = new EnvironmentConfig { Region = RegionEndpoint.USEast1 };
            var envOptions = Options.Create(envConfig);

            ILogger<SecretStoreReader> readerLogger = logFactory.CreateLogger<SecretStoreReader>();


            ISecretStoreReader paramStore = new SecretStoreReader(envOptions, readerLogger);

            string sftpValue = await paramStore.GetValueAsync("dev/discountproviderrx/sftpreport");

        }

        [Fact]
        public async Task FormatFileNameTest()
        {

            string fileName = "@@FormatUTC(yyyy-MM-ddTHHmmss)@@_Whetstone_Stateline_VoiceAssistantAPILog.csv";

            string retVal = await  MacroProcessing.ProcessMacrosAsync(fileName);




        }
    }
}
