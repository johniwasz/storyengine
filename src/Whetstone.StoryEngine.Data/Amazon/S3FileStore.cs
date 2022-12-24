
using Amazon.Runtime.Internal;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.MimeTypes;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Amazon
{
    public class S3FileStore : S3FileReader, IFileRepository
    {

        private readonly ILogger<S3FileStore> _dataLogger;

        private readonly IUserContextRetriever _contextRet;


        public S3FileStore(IOptions<EnvironmentConfig> envConfig, IUserContextRetriever userContextRetriever, IAmazonS3 s3Client, ILogger<S3FileStore> logger) : base(envConfig, s3Client, logger)
        {
            _dataLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextRet = userContextRetriever ?? throw new ArgumentNullException(nameof(userContextRetriever));
        }


        public async Task<T> GetJsonFileAsync<T>(TitleVersion titleVersion, string filePath)
        {

            return await GetJsonFileAsync<T>(titleVersion, filePath, null);

        }


        public async Task<T> GetJsonFileAsync<T>(TitleVersion titleVersion, string filePath, JsonSerializerSettings settings)
        {

            string internalPath = GetInternalPath(titleVersion, filePath, false);


            T returnContents = await ConvertJsonAsync<T>(internalPath, settings);
            return returnContents;
        }


        private async Task<T> ConvertJsonAsync<T>(string internalPath, JsonSerializerSettings settings)
        {
            T returnVal = default;


            string textContents = await S3Storage.GetConfigTextContentsAsync(_s3Client, _bucketName, internalPath);
            _dataLogger.LogDebug($"No cache available. Retrieved file {internalPath} from bucket {_bucketName}", internalPath);


            try
            {

                if (settings == null)
                    returnVal = JsonConvert.DeserializeObject<T>(textContents);
                else
                    returnVal = JsonConvert.DeserializeObject<T>(textContents, settings);
            }
            catch (Exception ex)
            {

                _dataLogger.LogError(ex, $"Error deserializing file {internalPath} from bucket {_bucketName}");

            }

            return returnVal;

        }





        public async Task<IEnumerable<AudioFileInfo>> GetAudioFileListAsync(Guid projectId, Guid versionId)
        {
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            List<AudioFileInfo> audioFileList = new AutoConstructedList<AudioFileInfo>();

            try
            {
                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                string audioPath = GetAudioPath(fileMapping);

                audioFileList = await S3Storage.ListAudioFileInfoAsync(_s3Client, _bucketName, audioPath);


            }
            catch (Exception ex)
            {

                _dataLogger.LogError(ex, $"Error getting audio files for projectid {projectId} and versionid {versionId}");
            }


            return audioFileList;

        }

        public async Task<AudioFileInfo> GetAudioFileInfoAsync(Guid projectId, Guid versionId, string fileName)
        {
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName cannot be empty.");
            }

            AudioFileInfo fileInfo = null;

            try
            {
                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                string audioPath = GetAudioPath(fileMapping);

                audioPath = $"{audioPath}/{fileName}";

                fileInfo = await S3Storage.GetAudioFileInfoAsync(_s3Client, _bucketName, audioPath, fileName);


            }
            catch (Exception ex)
            {

                // BUGBUG:TODO:SANJ - Workout with John the proper way to bubble up errors here
                _dataLogger.LogError(ex, $"Error getting audio file info for projectid {projectId} versionid {versionId} filename {fileName}");
            }


            return fileInfo;

        }

        public async Task DeleteAudioFileAsync(Guid projectId, Guid versionId, string fileName)
        {
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (String.IsNullOrEmpty(fileName))
                throw new ArgumentException("fileName cannot be empty.");

            string audioPath = String.Empty;

            try
            {
                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                audioPath = GetAudioPath(fileMapping);

                audioPath = $"{audioPath}/{fileName}";

                await S3Storage.DeleteAudioFileAsync(_s3Client, _bucketName, audioPath);


            }
            catch (Exception ex)
            {

                // BUGBUG:TODO:SANJ - Workout with John the proper way to bubble up errors here
                _dataLogger.LogError(ex, $"Error deleting file: {fileName} at path {audioPath} for projectid {projectId} and versionid {versionId}");
            }


        }

        public async Task<byte[]> GetFileContentAsync(TitleVersion titleVer, string fileName)
        {
            string internalPath = GetInternalPath(titleVer, fileName);


            byte[] returnContents = await S3Storage.GetFileFromStoreAsync(_s3Client, _bucketName, internalPath);
            _dataLogger.LogDebug($"No cache available. Retrieved file {internalPath} from bucket {_bucketName}");
            return returnContents;
        }

        public async Task<FileContent> GetFileContentAsync(Guid projectId, Guid versionId, string fileName)
        {

            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            FileContent retContents = null;

            try
            {
                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                string audioPath = GetAudioPath(fileMapping);

                audioPath = $"{audioPath}/{fileName}";

                byte[] binContents = await S3Storage.GetFileFromStoreAsync(_s3Client, _bucketName, audioPath);

                retContents = new FileContent
                {
                    Content = binContents,
                    FileName = fileName,
                    MimeType = GetMimeByFileName(fileName)
                };


            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Error getting audio file {fileName} for projectid {projectId} and versionid {versionId}");
            }


            return retContents;

        }

        public async Task<Stream> GetFileContentsAsync(string filePath)
        {
            return await S3Storage.GetFileStreamFromStoreAsync(_s3Client, _bucketName, filePath);
        }

        public async Task UploadFileContentsAsync(string filePath, string mimeType, Stream stm)
        {

            await S3Storage.StoreFileAsync(_s3Client, _bucketName, filePath, mimeType, stm);

        }

        public async Task<FileContentStream> GetFileContentStreamAsync(Guid projectId, Guid versionId, string fileName)
        {

            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            FileContentStream retContents = null;

            try
            {
                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                string audioPath = GetAudioPath(fileMapping);

                audioPath = $"{audioPath}/{fileName}";

                var binContents = await S3Storage.GetFileStreamFromStoreAsync(_s3Client, _bucketName, audioPath);

                retContents = new FileContentStream
                {
                    Content = binContents,
                    FileName = fileName,
                    MimeType = GetMimeByFileName(fileName)
                };


            }
            catch (Exception ex)
            {

                _dataLogger.LogError(ex, $"Error getting audio file {fileName} for projectid {projectId} and versionid {versionId}");
            }


            return retContents;



        }

        public async Task<AudioFileInfo> StoreAudioFileAsync(Guid projectId, Guid versionId, string fileName, Stream stm)
        {
            // BUGBUG:TODO:SANJ - This should choke under the following conditions:
            //
            // 1> The file is larger than a predefined limit
            // 2> The file is not a valid audio file - We need some way to validate this stuff
            //
            //
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (stm == null)
            {
                throw new ArgumentException($"Stream for file: {fileName} is null");
            }

            string audioPath;

            AudioFileInfo fileInfo;

            try
            {
                string mimeType = GetMimeByFileName(fileName);

                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                audioPath = GetAudioPath(fileMapping);

                audioPath = $"{audioPath}/{fileName}";

                _dataLogger.LogDebug($"Placing file {audioPath} in bucket {_bucketName}");
                await S3Storage.StoreFileAsync(_s3Client, _bucketName, audioPath, mimeType, stm);

                fileInfo = await S3Storage.GetAudioFileInfoAsync(_s3Client, _bucketName, audioPath, fileName);

            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Error storing audio file {fileName} for projectid {projectId} and versionid {versionId}");
                throw;
            }

            return fileInfo;
        }

        public async Task<UploadMediaFileInfo> UploadAudioFileAsync(Guid projectId, Guid versionId, string fileName, Stream stm)
        {
            // BUGBUG:TODO:SANJ - This should choke under the following conditions:
            //
            // 1> The file is larger than a predefined limit
            // 2> The file is not a valid audio file - We need some way to validate this stuff
            //
            //
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));

            if (stm == null)
                throw new ArgumentException($"Stream for file: {fileName} is null");

            string audioFileExtension = Path.GetExtension(fileName);
            Guid uploadFileId = Guid.NewGuid();
            string uploadFileName = String.Concat(uploadFileId, audioFileExtension);
            UploadMediaFileInfo fileInfo;

            try
            {
                string mimeType = GetMimeByFileName(fileName);

                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(projectId, versionId);

                string audioUploadPath = GetAudioUploadPath(fileMapping);
                audioUploadPath = $"{audioUploadPath}/{uploadFileName}";

                _dataLogger.LogDebug($"Placing file {audioUploadPath} in bucket {_bucketName}");
                await S3Storage.StoreFileAsync(_s3Client, _bucketName, audioUploadPath, mimeType, stm);

                fileInfo = new UploadMediaFileInfo
                {
                    FileType = MediaFileType.Audio,
                    ProjectId = projectId,
                    VersionId = versionId,
                    FileName = uploadFileName,
                    OriginalFileName = fileName
                };

            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Error storing audio file {fileName} for projectid {projectId} and versionid {versionId}");
                throw;
            }

            return fileInfo;
        }

        public async Task<AudioFileInfo> CommitUploadedAudioFileAsync(UploadMediaFileInfo uploadFileInfo)
        {
            // BUGBUG:TODO:SANJ - This should choke under the following conditions:
            //
            // 1> The file is larger than a predefined limit
            // 2> The file is not a valid audio file - We need some way to validate this stuff
            //
            //

            if (uploadFileInfo == null)
            {
                throw new ArgumentNullException(nameof(uploadFileInfo));
            }

            if (uploadFileInfo.FileType != MediaFileType.Audio)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(uploadFileInfo.FileType)}");
            }

            if (uploadFileInfo.ProjectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(uploadFileInfo.ProjectId)}");
            }

            if (uploadFileInfo.VersionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(uploadFileInfo.VersionId)}");
            }

            if (string.IsNullOrWhiteSpace(uploadFileInfo.OriginalFileName))
            {
                throw new ArgumentNullException(nameof(uploadFileInfo.OriginalFileName));
            }

            if (string.IsNullOrWhiteSpace(uploadFileInfo.FileName))
            {
                throw new ArgumentNullException(nameof(uploadFileInfo.FileName));
            }

            AudioFileInfo fileInfo;
            try
            {
                string mimeType = GetMimeByFileName(uploadFileInfo.OriginalFileName);

                ProjectVersionFileMapping fileMapping = await GetProjectVersionMapping(uploadFileInfo.ProjectId, uploadFileInfo.VersionId);

                string audioUploadPath = GetAudioUploadPath(fileMapping);
                audioUploadPath = $"{audioUploadPath}/{uploadFileInfo.FileName}";

                string destAudioPath = GetAudioPath(fileMapping);
                string destAudioFilePath = $"{destAudioPath}/{uploadFileInfo.OriginalFileName}";

                _dataLogger.LogDebug($"Moving file {audioUploadPath} to {destAudioPath} in bucket {_bucketName}");
                await S3Storage.RenameFile(_s3Client, _bucketName, audioUploadPath, destAudioFilePath);

                fileInfo = await S3Storage.GetAudioFileInfoAsync(_s3Client, _bucketName, destAudioFilePath, uploadFileInfo.OriginalFileName);

            }
            catch (Exception ex)
            {
                _dataLogger.LogError(ex, $"Error commmtting audio file {uploadFileInfo.OriginalFileName} from upload file: {uploadFileInfo.FileName} for projectid {uploadFileInfo.ProjectId} and versionid {uploadFileInfo.VersionId}");
                throw;
            }

            return fileInfo;
        }

        private async Task<ProjectVersionFileMapping> GetProjectVersionMapping(Guid projectId, Guid versionId)
        {
            if (projectId == default)
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            ProjectVersionFileMapping fileMapping;
            using (var userContext = await _contextRet.GetUserDataContextAsync())
            {
                fileMapping = await userContext.GetProjectVersionMapping(projectId, versionId);
            }


            return fileMapping;
        }




        public async Task SetTextContentAsync(string fileName, string contents, string mimeType)
        {
            await S3Storage.StoreFileAsync(_s3Client, _bucketName, fileName, mimeType, contents);
        }




        public async Task StoreFileAsync(TitleVersion titleVer, string fileName, byte[] contents)
        {
            string mimeType = GetMimeByFileName(fileName);

            string storagePath = GetInternalPath(titleVer, fileName, mimeType);

            await S3Storage.StoreFileAsync(_s3Client, _bucketName, storagePath, mimeType, contents);
            _dataLogger.LogDebug($"Placing file {storagePath} in bucket {_bucketName}");
        }

        public async Task StoreTitleAsync(StoryTitle title)
        {
            string mimeType = MimeTypeMap.GetMimeType("yaml");
            TitleVersion titleVer = new TitleVersion(title.Id, title.Version);


            string titleIdPath = this.GetTitlePath(titleVer);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(title);
            await S3Storage.StoreFileAsync(_s3Client, _bucketName, titleIdPath, mimeType, yamlOut);


        }


        public async Task<bool> DoesFileExistAsync(TitleVersion titleVer, string fileName)
        {

            string internalPath = GetInternalPath(titleVer, fileName);

            bool fileExists = await S3Storage.DoesFileExistAsync(_s3Client, _bucketName, internalPath);
            _dataLogger.LogDebug($"Checking if file {internalPath} exists in bucket {_bucketName}. Result is {fileExists}");
            return fileExists;
        }

        public async Task<List<string>> GetAudioFileListAsync(TitleVersion titleVer)
        {

            string audioPath = GetAudioPath(titleVer);

            List<string> fileList = await S3Storage.ListFilesAsync(_s3Client, _bucketName, audioPath);



            return fileList;
        }

        public async Task<string> GetTextContentAsync(string fileName)
        {
            string textContents = await S3Storage.GetConfigTextContentsAsync(_s3Client, _bucketName, fileName);
            return textContents;
        }


        public async Task CopyMediaFilesAsync(string titleId, string sourceVersion, string destVersion)
        {


            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            if (string.IsNullOrWhiteSpace(destVersion))
                throw new ArgumentNullException(nameof(destVersion));

            TitleVersion sourceTitleVer = new TitleVersion(titleId, sourceVersion);
            TitleVersion destTitleVer = new TitleVersion(titleId, destVersion);


            string sourceImagePath = GetImagePath(sourceTitleVer);

            // Does the path exist?
            if (await S3Storage.DoesFileExistAsync(_s3Client, _bucketName, sourceImagePath))
            {
                // images exist. Copy them.
                string destImagePath = GetImagePath(destTitleVer);

                await S3Storage.CopyFileDirectoryAsync(_s3Client, _bucketName, sourceImagePath, destImagePath, _dataLogger);
            }

            string sourceAudioPath = GetAudioPath(sourceTitleVer);

            if (await S3Storage.DoesFileExistAsync(_s3Client, _bucketName, sourceAudioPath))
            {
                // images exist. Copy them.
                string destAudioPath = GetAudioPath(destTitleVer);

                await S3Storage.CopyFileDirectoryAsync(_s3Client, _bucketName, sourceAudioPath, destAudioPath, _dataLogger);
            }

        }

        public async Task PurgeTitleAsync(TitleVersion titleVer)
        {

            string titleId = titleVer.ShortName;
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentException($"ShortName in {nameof(titleVer)} cannot be null or empty");

            string titleFile = GetTitlePath(titleVer);
            // Delete the title file

            string deletedTitleFile = GetDeletedFileName(titleFile);

            await S3Storage.DeleteFile(_s3Client, _bucketName, titleFile);

            await S3Storage.DeleteFile(_s3Client, _bucketName, deletedTitleFile);


            string imagePath = GetImagePath(titleVer);

            await S3Storage.DeleteDirectoryAsync(_s3Client, _bucketName, imagePath);

            string audioPath = GetAudioPath(titleVer);

            await S3Storage.DeleteDirectoryAsync(_s3Client, _bucketName, audioPath);


        }


        public async Task DeleteTitleAsync(TitleVersion titleVer)
        {


            string titleId = titleVer.ShortName;
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentException($"ShortName in {nameof(titleVer)} cannot be null or empty");

            string titleFile = GetTitlePath(titleVer);


            if (await S3Storage.DoesFileExistAsync(_s3Client, _bucketName, titleFile))
            {
                string titleFileDeleteName = GetDeletedFileName(titleFile);

                // Delete the title file
                await S3Storage.RenameFile(_s3Client, _bucketName, titleFile, titleFileDeleteName);
            }

        }

        private string GetDeletedFileName(string fileName)
        {
            return string.Concat(fileName, ".deleted");

        }
    }
}
