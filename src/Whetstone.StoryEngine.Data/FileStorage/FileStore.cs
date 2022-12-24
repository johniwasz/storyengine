using Amazon.S3;
using Microsoft.Extensions.Options;
using System;
using Whetstone.StoryEngine.Data.MimeTypes;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.FileStorage
{
    public abstract class FileStore : FileLinkStore
    {
        private const string STORIES_FOLDER = "stories";
        private const string UPLOADS_FOLDER = "uploads";

        protected string _bucketName;

        protected readonly IAmazonS3 _s3Client;

        protected FileStore(IOptions<EnvironmentConfig> envConfig, IAmazonS3 s3Client) : base()
        {
            if (envConfig == null)
                throw new ArgumentNullException(nameof(envConfig));

            if (envConfig.Value == null)
                throw new ArgumentNullException(nameof(envConfig), "Value property cannot be null");

            _bucketName = envConfig.Value.BucketName;

            if (string.IsNullOrWhiteSpace(_bucketName))
            {
                throw new ArgumentNullException(nameof(envConfig), "BucketName setting cannot be null or empty");
            }

            _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));

        }




        protected string GetAudioPath(TitleVersion titleVer)
        {
            return GetMediaPath(titleVer, "mp3", STORIES_FOLDER);
        }

        protected string GetAudioPath(ProjectVersionFileMapping fileMappingInfo)
        {
            return GetMediaPath(fileMappingInfo, "mp3", STORIES_FOLDER);
        }

        protected string GetAudioUploadPath(TitleVersion titleVer)
        {
            return GetMediaPath(titleVer, "mp3", UPLOADS_FOLDER);
        }

        protected string GetAudioUploadPath(ProjectVersionFileMapping fileMappingInfo)
        {
            return GetMediaPath(fileMappingInfo, "mp3", UPLOADS_FOLDER);
        }

        protected string GetImagePath(TitleVersion titleVer)
        {
            return GetMediaPath(titleVer, "jpg", STORIES_FOLDER);
        }

        protected string GetImageUploadPath(TitleVersion titleVer)
        {
            return GetMediaPath(titleVer, "jpg", UPLOADS_FOLDER);
        }

        private string GetMediaPath(TitleVersion titleVer, string fileExt, string baseFolder)
        {

            if (titleVer == null)
                throw new ArgumentNullException(nameof(titleVer));

            if (string.IsNullOrWhiteSpace(titleVer.Version))
            {
                throw new ArgumentNullException(nameof(titleVer), "Version property cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(titleVer.ShortName))
            {
                throw new ArgumentNullException(nameof(titleVer), "ShortName property cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(fileExt))
                throw new ArgumentNullException(nameof(fileExt));

            ProjectVersionFileMapping fileMapping = new ProjectVersionFileMapping
            {
                Version = titleVer.Version,
                ProjectAlias = titleVer.ShortName
            };


            return GetMediaPath(fileMapping, fileExt, baseFolder);


        }


        private string GetMediaPath(ProjectVersionFileMapping fileMapping, string fileExt, string baseFolder)
        {
            if (fileMapping == null)
                throw new ArgumentNullException(nameof(fileMapping));

            if (string.IsNullOrWhiteSpace(fileMapping.Version))
            {
                throw new ArgumentNullException(nameof(fileMapping), "Version property cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(fileMapping.ProjectAlias))
            {
                throw new ArgumentNullException(nameof(fileMapping), "ProjectAlias property cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(fileExt))
                throw new ArgumentNullException(nameof(fileExt));

            var mimeType = MimeTypeMap.GetMimeType(fileExt);
            var mimeCat = GetMimeCategory(mimeType);
            string mediaPath;

            if (string.IsNullOrWhiteSpace(fileMapping.Version))
                mediaPath = string.Concat(baseFolder, @"/", fileMapping.ProjectAlias, @"/", mimeCat);
            else
                mediaPath = string.Concat(baseFolder, @"/", fileMapping.ProjectAlias, @"/", fileMapping.Version, @"/", mimeCat);


            return mediaPath;
        }


    }


}
