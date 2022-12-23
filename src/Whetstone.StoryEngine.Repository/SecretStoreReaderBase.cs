using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Messaging;

namespace Whetstone.StoryEngine.Repository
{
    public abstract class SecretStoreReaderBase<T>
    {
        private readonly ISecretStoreReader _secretStoreReader;

        private readonly IMemoryCache _memCache;

        protected SecretStoreReaderBase(ISecretStoreReader secureStoreReader, IMemoryCache memCache)
        {
            _secretStoreReader = secureStoreReader ??
                                 throw new ArgumentException(
                                     $"{nameof(secureStoreReader)} cannot be null");

            _memCache = memCache ?? throw new ArgumentException($"{nameof(memCache)} cannot be null");

        }

        protected async Task<T> GetCredentialsAsync(string credentialStoreName)
        {
            if (string.IsNullOrWhiteSpace(credentialStoreName))
                throw new ArgumentException($"{nameof(credentialStoreName)} cannot be null or empty");

            T creds = default(T);

            try
            {
                if (!_memCache.TryGetValue(credentialStoreName, out creds))
                {
                    string storeValue = await _secretStoreReader.GetValueAsync(credentialStoreName);
                    if (!string.IsNullOrWhiteSpace(storeValue))
                    {
                        creds = JsonConvert.DeserializeObject<T>(storeValue);
                        _memCache.Set(credentialStoreName, creds);
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception($"Error getting credentials {credentialStoreName}", ex);
            }

            return creds;

        }


    }
}
