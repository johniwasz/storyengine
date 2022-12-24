using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class S3MediaLinker : FileLinkStore, IMediaLinker
    {


        private string _bucketName;

        public S3MediaLinker(IOptions<EnvironmentConfig> envConfig) : base()
        {
            if (envConfig == null)
                throw new ArgumentNullException(nameof(envConfig));

            if (envConfig.Value == null)
                throw new ArgumentNullException(nameof(envConfig), "Value property cannot be null or empty");


            _bucketName = envConfig.Value.BucketName;

            if (string.IsNullOrWhiteSpace(_bucketName))
                throw new ArgumentNullException(nameof(envConfig), "BucketName setting cannot be null or empty");

        }


        public async Task<string> GetFileLinkAsync(TitleVersion titleVersion, string fileName)
        {


            string internalPath = GetInternalPath(titleVersion, fileName);

            string bucketHost = string.Concat(_bucketName,
                ".s3.amazonaws.com");

            UriBuilder s3Uri = new UriBuilder("https", bucketHost)
            {
                Path = internalPath
            };

            return await Task.FromResult<string>(s3Uri.ToString());


        }

        public string GetFileLink(TitleVersion titleVersion, string fileName)
        {

            string internalPath = GetInternalPath(titleVersion, fileName);

            string bucketHost = string.Concat(_bucketName,
                ".s3.amazonaws.com");

            UriBuilder s3Uri = new UriBuilder("https", bucketHost);

            s3Uri.Path = internalPath;

            return s3Uri.ToString();


        }




    }
}
