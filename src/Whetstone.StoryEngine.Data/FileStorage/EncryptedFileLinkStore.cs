
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Data.FileStorage
{
    public abstract class EncryptedFileLinkStore : FileLinkStore
    {

        protected byte[] _iv;
        protected byte[] _keyAes;

        protected TimeSpan _timeOut;

        protected string _mediaRoute;


        public EncryptedFileLinkStore(IOptions<EncryptionConfig> encryptConfig) : base()
        {
            int timeoutSec = encryptConfig.Value.LinkTimeout.GetValueOrDefault(300);

            _timeOut = new TimeSpan(0, 0, timeoutSec);

            _mediaRoute = encryptConfig.Value.MediaRoute;

            if (encryptConfig.Value != null)
            {
                if(!string.IsNullOrWhiteSpace(encryptConfig.Value.KeyVector))
                    _iv = Convert.FromBase64String(encryptConfig.Value.KeyVector);


                if (!string.IsNullOrWhiteSpace(encryptConfig.Value.Key))
                    _keyAes = Convert.FromBase64String(encryptConfig.Value.Key);
            }
        }



    }
}
