using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Repository
{
    public class LocalFileReader : FileLinkStore, IFileReader
    {
        private readonly ILogger<LocalFileReader> _dataLogger;
        private readonly string _localFilePath;
        private readonly IDeserializer _yamlSerializer;

        public LocalFileReader(IOptions<LocalFileConfig> localFileConfig, ILogger<LocalFileReader> logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            IOptions<LocalFileConfig> locConfig =
                localFileConfig ?? throw new ArgumentNullException(nameof(localFileConfig));

            if (string.IsNullOrWhiteSpace(locConfig.Value?.RootPath))
            {
                throw new ArgumentNullException(nameof(localFileConfig), "RootPath cannot be empty");
            }

            _localFilePath = locConfig.Value.RootPath;

            _yamlSerializer = YamlSerializationBuilder.GetYamlDeserializer();

        }

        public Task<StoryTitle> GetTitleContentsAsync(TitleVersion titleVersion)
        {
            string partialLocalPath = GetDebugTitlePath(titleVersion);


            string fullPath = Path.Combine(_localFilePath, partialLocalPath);
            StoryTitle locTitle;


            if (File.Exists(fullPath))
            {
                string fileText = File.ReadAllText(fullPath);
                locTitle = _yamlSerializer.Deserialize<StoryTitle>(fileText);
            }
            else
            {
                string errMsg = $"Title file not found at {fullPath}";
                _dataLogger.LogError(errMsg);
                var tcs = new TaskCompletionSource<StoryTitle>();
                tcs.SetException(new FileNotFoundException(errMsg));
                return tcs.Task;
            }


            return Task.FromResult(locTitle);
        }
    }
}
