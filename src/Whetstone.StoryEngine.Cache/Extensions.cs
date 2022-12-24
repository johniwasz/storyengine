using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Cache.Models;

namespace Whetstone.StoryEngine.Cache
{
    public static class Extensions
    {
        //public static T GetObject<T>(this IDistributedCache cache, string key)
        //{
        //    byte[] contents =  cache.Get(key);
        //    T retVal = Deserialize<T>(contents);
        //    return retVal;
        //}

        public static async Task<T> GetObjectAsync<T>(this IDistributedCache cache, string key)
        {

            byte[] contents = await cache.GetAsync(key);
            T retVal = Deserialize<T>(contents);
            return retVal;
        }

        //public static void SetObject<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions cacheOptions)
        //{
        //    byte[] byteVal = Serialize(key, value);
        //    cache.Set(key, byteVal, cacheOptions);
        //}


        public static async Task SetObjectAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions cacheOptions)
        {
            byte[] byteVal = Serialize(key, value);
            await cache.SetAsync(key, byteVal, cacheOptions);
        }

        private static T Deserialize<T>(byte[] contents)
        {
            T retVal = default;


            if (contents != null)
            {
                using (MemoryStream memStream = new(contents))
                {
                    retVal = MessagePack.MessagePackSerializer.Deserialize<T>(memStream, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);                    
                }
            }
            return retVal;

        }
        private static byte[] Serialize<T>(string key, T value)
        {
            byte[] byteVal = null;
            using (MemoryStream memStream = new())
            {
                try
                {

                    MessagePack.MessagePackSerializer.Serialize<T>(memStream, value, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);
                    //  ProtoBuf.Serializer.Serialize<T>(memStream, val);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error serializing item to memory stream with key {key}", ex);

                }
                byteVal = memStream.ToArray();
            }

            return byteVal;
        }



    }
}
