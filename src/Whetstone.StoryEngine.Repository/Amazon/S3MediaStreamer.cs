using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class S3MediaStreamer : EncryptedFileLinkStore, IMediaStreamer
    {

        private RegionEndpoint _defaultEndpoint = null;

        private string _bucketName;

        public S3MediaStreamer(IOptions<EncryptionConfig> encryptConfig, IOptions<EnvironmentConfig> envOptions) : base(encryptConfig)
        {

            IOptions<EnvironmentConfig> envOpts =
                envOptions ?? throw new ArgumentNullException(nameof(envOptions));

            EnvironmentConfig envConfig = envOptions.Value ??
                                          throw new ArgumentNullException(
                                              nameof(envOptions), "Value property cannot be null");


            _defaultEndpoint = envConfig.Region ??
                               throw new ArgumentNullException(nameof(envOptions), "Value property cannot be null");

            _bucketName = envConfig.BucketName;

            if (string.IsNullOrWhiteSpace(_bucketName))
                throw new ArgumentException(nameof(envOptions), "BucketName setting cannot be null or empty");

        }




        public async Task<SimpleMediaResponse> GetFileStreamAsync(string environment, TitleVersion titleVer, string fileName, bool isFileNameEncrypted = true)
        {

            string plainFileName = isFileNameEncrypted ? GetDecodedFileName(fileName) : fileName;
            SimpleMediaResponse mediaResponse = null;
            if (!string.IsNullOrWhiteSpace(plainFileName))
            {

                string internalPath = GetInternalPath(titleVer, plainFileName);
                string contentType = GetMimeByFileName(plainFileName);
                var memStream = new MemoryStream();

                using (IAmazonS3 client = new AmazonS3Client(_defaultEndpoint))
                {

                    GetObjectRequest request = new GetObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = internalPath
                    };

                    using (GetObjectResponse response = await client.GetObjectAsync(request))
                    {
                        await response.ResponseStream.CopyToAsync(memStream);
                    }
                }
                mediaResponse = new SimpleMediaResponse(memStream, contentType);

                mediaResponse.MediaStream.Position = 0;

            }
            return mediaResponse;
        }


        protected MediaLink DecryptMediaLink(string mediaLinkEncrypted)
        {
            // Check arguments.
            if (mediaLinkEncrypted == null || mediaLinkEncrypted.Length <= 0)
                throw new ArgumentNullException($"{nameof(mediaLinkEncrypted)}");

            MediaLink mediaInfo = null;

            byte[] mediaBytes = Convert.FromBase64String(mediaLinkEncrypted);

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _keyAes;
                aesAlg.IV = _iv;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(mediaBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        mediaInfo = MessagePack.MessagePackSerializer.Deserialize<MediaLink>(msDecrypt, MessagePack.Resolvers.ContractlessStandardResolver.Options);
                    }
                }

            }

            return mediaInfo;

        }

        protected string GetDecodedFileName(string encodedFile)
        {
            string urldecoded = System.Net.WebUtility.UrlDecode(encodedFile);
            MediaLink mediaLink = DecryptMediaLink(encodedFile);

            // Check the expiration time.

            if (mediaLink.AbsoluteExpireTime >= DateTime.UtcNow)
                return mediaLink.FileName;



            return null;

        }
    }
}
