using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.AudioProcessor;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.AudioProcessor.Repository
{
    public class AudioFileHandler : IAudioFileHandler
    {
        private ILogger<AudioFileHandler> _logger;
        private IAudioProcessor _audioProcessor;
        private IFileRepository _fileRepository;
        private AudioProcessorConfig _config;

        public AudioFileHandler(ILogger<AudioFileHandler> logger, IAudioProcessor audioProcessor, IFileRepository fileRepository, IOptions<AudioProcessorConfig> config)
        {
            _logger = logger;
            _config = config.Value;

            if (String.IsNullOrEmpty(_config.TempFolder))
                throw new ArgumentNullException("AudioProcessorConfig::TempFolder cannot be empty!");

            _audioProcessor = audioProcessor;
            _fileRepository = fileRepository;
        }

        public async Task<string> ProcessAudioFile(string fileName, string filePath, string destFileName, string destFilePath)
        {
            string sRet = null;
            string sourceFilePath = Path.Combine(_config.TempFolder, fileName);
            string convertedFilePath = Path.Combine(_config.TempFolder, destFileName);


            try
            {
                // Copy the file locally from the file repository then figure out
                // what kind of file we have
                await DownloadLocalFileFromFileRepository(filePath, sourceFilePath);

                _logger.LogDebug($"Getting audio file info for: {sourceFilePath}");
                IAudioFileInfo audioInfo = _audioProcessor.GetAudioFileInfo(sourceFilePath);

                // The file must be an audio file or we're done. If it is, it must be one
                // valid for playback - if it is not, then we must convert it.
                if (audioInfo.IsAudioFile)
                {
                    if (!audioInfo.IsValidAudioFile)
                    {
                        _logger.LogDebug($"Audio File: {fileName} not valid, converting.");

                        _audioProcessor.ConvertAudioFile(sourceFilePath, convertedFilePath);

                        _logger.LogDebug($"Audio File converted to: {convertedFilePath} preparing to upload");
                        await UploadLocalFileToFileRepository(destFilePath, convertedFilePath);

                        sRet = $"Audio File: {fileName} converted and uploaded to: {destFilePath}";

                        // BUGBUG:TODO:SANJ - Nuke the source audio file and the original audio file
                    }
                    else
                    {
                        _logger.LogDebug($"Audio File: {fileName} is a valid audio file.");
                        sRet = $"Audio File: {fileName} is a valid audio file - nothing to do.";

                        // BUGBUG:TODO:SANJ - Move the audio file from the source path to the destination path
                    }

                }
                else
                {
                    _logger.LogDebug($"Audio File: {fileName} is not a valid audio file.");
                    sRet = $"Audio File: {fileName} is not a valid audio file";

                    // BUGBUG:TODO:SANJ - Nuke the audio file from the temp folder
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing audio file.");
                throw;
            }
            finally
            {
                // Always clean up any local files we leave lying around.
                if (File.Exists(sourceFilePath))
                {
                    _logger.LogDebug($"Deleting file: {sourceFilePath}");
                    File.Delete(sourceFilePath);
                }

                if (File.Exists(convertedFilePath))
                {
                    _logger.LogDebug($"Deleting file: {convertedFilePath}");
                    File.Delete(convertedFilePath);
                }
            }

            return sRet;
        }

        private async Task UploadLocalFileToFileRepository(string destFilePath, string convertedFilePath)
        {
            using (Stream destStm = File.OpenRead(convertedFilePath))
            {
                _logger.LogDebug($"Uploading file: {convertedFilePath} to: {destFilePath}");
                await _fileRepository.UploadFileContentsAsync(destFilePath, "audio/mpeg", destStm);
            }
        }

        private async Task DownloadLocalFileFromFileRepository(string filePath, string sourceFilePath)
        {
            using (Stream s = await _fileRepository.GetFileContentsAsync(filePath))
            {
                byte[] abData = new byte[s.Length];

                _logger.LogDebug($"Reading file contents from stream");
                s.Read(abData, 0, (int)s.Length);

                _logger.LogDebug($"Writing file contents to temp folder: {sourceFilePath}");
                File.WriteAllBytes(sourceFilePath, abData);
            }
        }
    }
}
