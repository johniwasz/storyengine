using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.AudioProcessor.Repository;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.AudioProcessor;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.WebLibrary;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Whetstone.StoryEngine.AudioProcessor.Lambda
{
    public class AudioProcessorEntry : ApiLambdaBase
    {
        private const string AUDIOTEMPFOLDER = "AUDIOTEMPFOLDER";
        private const string FFMPEGPATH = "FFMPEGPATH";

        IFileRepository _fileRepository;
        private readonly ILogger<AudioProcessorEntry> _logger;
        private readonly IAudioFileHandler _audioFileHandler;

        public AudioProcessorEntry()
            : base()
        {
            _logger = this.Services.GetService<ILogger<AudioProcessorEntry>>();
            _fileRepository = this.Services.GetService<IFileRepository>();
            _audioFileHandler = this.Services.GetService<IAudioFileHandler>();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<string> FunctionHandler(AudioProcessorTest testFile, ILambdaContext context)
        {
            _logger.LogDebug($"AudioProcessorEntry::FunctionHandler FIleName: {testFile.FileName}, path: {testFile.FilePath}, destFileName: {testFile.DestFileName}, destFilePath: {testFile.DestFilePath}");

            string sRet = await _audioFileHandler.ProcessAudioFile(testFile.FileName, testFile.FilePath, testFile.DestFileName, testFile.DestFilePath);

            _logger.LogDebug($"AudioProcessorEntry::FunctionHandler return: {sRet}");

            return sRet;
        }

        protected override void ConfigureServices(IServiceCollection services, IConfiguration config, BootstrapConfig bootConfig)
        {
            // Setup the base services first!
            base.ConfigureServices(services, config, bootConfig);

            // Get our path options from the appropriate environment variables
            services.Configure<AudioProcessorConfig>(x =>
            {
                x.TempFolder = Environment.GetEnvironmentVariable(AUDIOTEMPFOLDER);
                x.FFMpegPath = Environment.GetEnvironmentVariable(FFMPEGPATH);
            });

            services.AddSingleton<IAudioProcessor, FFMpegAudioProcessor>();
            services.AddSingleton<IAudioFileHandler, AudioFileHandler>();
        }
    }

}
