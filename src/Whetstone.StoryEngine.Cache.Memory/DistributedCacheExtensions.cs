using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Whetstone.StoryEngine.Cache.Memory
{
    public static class DistributedCacheExtensions
    {

        //private static readonly bool _isEnabled = true;

        //private static async Task SetAsync<T>(this IDistributedCache cache, string key, T val, DistributedCacheEntryOptions cacheOptions)
        //{


        //    if (_isEnabled)
        //    {
        //        byte[] byteVal = null;
        //        using (MemoryStream memStream = new MemoryStream())
        //        {
        //            try
        //            {

        //                MessagePack.MessagePackSerializer.Serialize<T>(memStream, val, MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
        //                //  ProtoBuf.Serializer.Serialize<T>(memStream, val);
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception($"Error serializing item to memory stream with key {key}", ex);

        //            }
        //            byteVal = memStream.ToArray();
        //        }


        //        await cache.SetAsync(key, byteVal, cacheOptions);
        //    }
        //}




        //private static async Task<T> GetAsync<T>(this IDistributedCache cache, string key)
        //{
        //    if (_isEnabled)
        //    {
        //        T retVal = default(T);
        //        ;

        //        byte[] contents = await cache.GetAsync(key);
        //        if (contents != null)
        //        {
        //            using (MemoryStream memStream = new MemoryStream(contents))
        //            {
        //                retVal = MessagePack.MessagePackSerializer.Deserialize<T>(memStream, MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
        //                //retVal = ProtoBuf.Serializer.Deserialize<T>(memStream);
        //            }
        //        }
        //        return retVal;
        //    }
        //    else
        //        return default(T);
        //}

    }
}
