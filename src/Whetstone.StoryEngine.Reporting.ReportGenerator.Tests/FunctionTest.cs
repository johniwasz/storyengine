using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Reporting.ReportGenerator;
using Whetstone.StoryEngine.Repository;
using Whetstone.StoryEngine.Repository.Amazon;
using SftpClient = Whetstone.StoryEngine.Repository.SftpClient;
using ISftpClient = Whetstone.StoryEngine.Repository.ISftpClient;
using Microsoft.Extensions.Logging;

namespace Whetstone.StoryEngine.Reporting.ReportGenerator.Tests
{
    public class FunctionTest
    {
        public FunctionTest()
        {
        }

        [Fact]
        public async Task GetGoodSftpConfigAsync()
        {

            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();

            });


            var storeLogger = logFactory.CreateLogger<ParameterStoreReader>();

            EnvironmentConfig envConfig = new EnvironmentConfig { Region = RegionEndpoint.USEast1 };
            var envOptions = Options.Create(envConfig);

            IParameterStoreReader paramStore = new ParameterStoreReader(envOptions, storeLogger);

            string paramName = @"/somediscountproviderrx/dev/sftpreport";

            string paramValue = await paramStore.GetValueAsync(paramName);

            Assert.NotNull(paramValue);

            var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

            SftpConfig ftpConfig = yamlDeser.Deserialize<SftpConfig>(paramValue);


            var connectionInfo = new ConnectionInfo(ftpConfig.Host,
                ftpConfig.UserName,
                new PasswordAuthenticationMethod(ftpConfig.UserName,ftpConfig.UserSecret));

            string fileName = DateTime.UtcNow.ToString("yyyyMMdd");

            fileName = string.Concat("SYM-", fileName, "-02.csv");
            string fileContents = null;
            using (var client = new Renci.SshNet.SftpClient(connectionInfo))
            {
                client.Connect();

                var dirList = client.ListDirectory(".");

                StringBuilder csvContents = new StringBuilder();
               
                using (MemoryStream memStream = new MemoryStream())
                { 
                    client.DownloadFile("sampleexport.csv", memStream);
                    memStream.Position = 0;
                    using (var reader = new StreamReader(memStream))
                    {
                        fileContents = reader.ReadToEnd();
                    }
                }

                if (!string.IsNullOrWhiteSpace(fileContents))
                {

                    byte[] fileBytes = Encoding.UTF8.GetBytes(fileContents);
                    using (MemoryStream upFileStream = new MemoryStream(fileBytes))
                    {
                        client.UploadFile(upFileStream, fileName);
                    }
                }
            }


            var ftpLogger = logFactory.CreateLogger<SftpClient>();

            ISftpClient ftpClient = new SftpClient(ftpLogger);

            await ftpClient.UploadFileAsync(ftpConfig, fileName, fileContents);

        }

        [Fact]
        public async Task GetSftpConfigAsync()
        {

            ILoggerFactory logFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();

            });


            var storeLogger = logFactory.CreateLogger<ParameterStoreReader>();

            EnvironmentConfig envConfig = new EnvironmentConfig {Region = RegionEndpoint.USEast1};


            var envOptions = Options.Create(envConfig);

            IParameterStoreReader paramStore = new ParameterStoreReader(envOptions, storeLogger);

            string paramValue = await paramStore.GetValueAsync("doesnotexist");

            // Parameter value should be null. The parameter does not exits.
            Assert.Null(paramValue);

        }


    }
}
