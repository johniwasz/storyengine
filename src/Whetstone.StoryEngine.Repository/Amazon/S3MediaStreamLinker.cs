using MessagePack;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository.Amazon
{
    public class S3MediaStreamLinker : EncryptedFileLinkStore, IMediaLinker
    {


        public S3MediaStreamLinker(IOptions<EncryptionConfig> encryptConfig) : base(encryptConfig)
        {


        }

        public string GetFileLink(TitleVersion titleVer, string fileName)
        {

            string timedLink = GetTimedLink(titleVer, fileName);

            return timedLink;
        }

        protected string GetTimedLink(TitleVersion titleVer, string fileName)
        {
            // Concatenate the url with the title and file name.
            string mediaLink = _mediaRoute;

            if (!mediaLink.EndsWith("/") && !string.IsNullOrWhiteSpace(titleVer.ShortName))

            {
                mediaLink = string.Concat(mediaLink, '/');
            }
            else if (mediaLink.EndsWith("/"))
            {
                mediaLink = mediaLink.Substring(0, mediaLink.Length - 1);
            }

            DateTime absoluteExpireTime = DateTime.UtcNow.Add(_timeOut);

            //  var buffer = Encoding.UTF8.GetBytes(fileConcat);
            MediaLink mediaRef = new MediaLink(fileName, absoluteExpireTime);

            string encryptedInfo = EncryptMediaLink(mediaRef);

            string routePart = System.Net.WebUtility.UrlEncode(encryptedInfo);

            // TODO Include version
            mediaLink = string.Concat(mediaLink, titleVer.ShortName, "?t=", routePart);

            return mediaLink;
        }


        protected string EncryptMediaLink(MediaLink mediaInfo)
        {
            // Check arguments.
            if (mediaInfo == null)
                throw new ArgumentNullException($"{nameof(mediaInfo)}");


            string encryptedString;
            // Create an Aes object
            // with the specified key and IV.

            using (var memStream = new MemoryStream())
            {

                MessagePackSerializer.Serialize<MediaLink>(memStream, mediaInfo);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = _keyAes;
                    aesAlg.IV = _iv;

                    // Create a decrytor to perform the stream transform.
                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {

                            memStream.CopyTo(csEncrypt);


                            encryptedString = Convert.ToBase64String(memStream.ToArray());

                        }
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encryptedString;

        }



    }
}
