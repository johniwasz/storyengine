using System;
using System.Diagnostics;
using System.IO;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Whetstone.StoryEngine.Models.AudioProcessor;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.AudioProcessor.Repository
{
    public class FFMpegAudioProcessor : IAudioProcessor
    {
        private const int EXIT_CODE_SUCCESS = 0;

        private const string FFPROBE = "ffprobe";
        private const string FFMPEG = "ffmpeg";

        private readonly string FFProbeExePath;
        private readonly string FFMpegExePath;

        private ILogger<FFMpegAudioProcessor> _logger;
        private AudioProcessorConfig _config;

        public FFMpegAudioProcessor( ILogger<FFMpegAudioProcessor> logger, IOptions<AudioProcessorConfig> config )
        {
            _logger = logger;
            _config = config.Value;

            if (String.IsNullOrEmpty(_config.FFMpegPath))
                throw new ArgumentNullException("AudioProcessorConfig::FFMpegPath cannot be empty!");

            FFProbeExePath = Path.Combine(_config.FFMpegPath, FFPROBE);
            FFMpegExePath = Path.Combine(_config.FFMpegPath, FFMPEG);

            _logger.LogDebug($"FFProbe Path: {FFProbeExePath}");
            _logger.LogDebug($"FFMpeg Path: {FFMpegExePath}");
        }

        // Use the ffprobe binary to read the contents of the file at the specified path
        // and return information about the file.

        public IAudioFileInfo GetAudioFileInfo( string inputFile )
        {
            Process process = new Process();
            process.StartInfo.FileName = FFProbeExePath;
            process.StartInfo.Arguments = inputFile;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if ( process.ExitCode != EXIT_CODE_SUCCESS )
            {
                _logger.LogError($"ffprobe returned nonzero exit code: {process.ExitCode}.\n{err}");
                throw new InvalidOperationException($"Unable to execute ffprobe on input file: {inputFile}, exit code: {process.ExitCode}");
            }

            _logger.LogDebug($"Parsing audio file info for: {inputFile}");
            return new FFProbeAudioFileInfo(err);

        }

        public void ConvertAudioFile( string inputFile, string outputFile )
        {
            Process process = new Process();
            process.StartInfo.FileName = FFMpegExePath;

            // This command line will create an mp3 file that is compatible with Alexa and Google Assistant
            process.StartInfo.Arguments = $"-nostdin -i {inputFile} -ac 2 -codec:a libmp3lame -b:a 48k -ar 24000 -write_xing 0 {outputFile}"; // Note the /c command (*)

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            //* Read the output (or the error)
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != EXIT_CODE_SUCCESS)
            {
                _logger.LogError($"ffmpeg returned nonzero exit code: {process.ExitCode} converting input file: {inputFile} to output file {outputFile}.\n{err}");
                throw new InvalidOperationException($"Unable to run ffmpeg on input file: {inputFile} to convert to output file: {outputFile}, exit code: {process.ExitCode}");
            }

        }
    }
}
