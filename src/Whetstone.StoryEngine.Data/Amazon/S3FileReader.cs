using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Amazon
{
    public class S3FileReader : FileStore, IFileReader
    {
        private readonly ILogger<S3FileReader> _dataLogger;

        public S3FileReader(IOptions<EnvironmentConfig> envConfig, IAmazonS3 s3Client, ILogger<S3FileReader> logger) : base(envConfig, s3Client)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task<StoryTitle> GetTitleContentsAsync(TitleVersion titleVer)
        {
            string titlePath = GetTitlePath(titleVer);
            StoryTitle title = await GetTitleAsync(titlePath);

            return title;

        }

        private async Task<StoryTitle> GetTitleAsync(string titlePath)
        {
            string titleText = await S3Storage.GetConfigTextContentsAsync(_s3Client, _bucketName, titlePath);

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle title = yamlDeserializer.Deserialize<StoryTitle>(titleText);
            if ((title.Intents?.Any()).GetValueOrDefault(false))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                title.Intents = title.Intents.OrderBy(x => x.Name).ToList();

            }


            if ((title.Nodes?.Any()).GetValueOrDefault(false))
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                title.Nodes = title.Nodes.OrderBy(x => x.Name).ToList();

            }



            _dataLogger.LogDebug($"Getting config for title {titlePath} from bucket {_bucketName}");

            return title;
        }

    }
}
