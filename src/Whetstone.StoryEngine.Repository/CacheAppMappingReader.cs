using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.RDS.Model;
using Microsoft.Extensions.Caching.Memory;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;
using Microsoft.Extensions.Options;

namespace Whetstone.StoryEngine.Repository
{
    public class CacheAppMappingReader : IAppMappingReader
    {

        private readonly ITitleCacheRepository _titleCacheRep;
  
        //private static InMe

        public CacheAppMappingReader(ITitleCacheRepository titleCacheRep)
        {
            _titleCacheRep =
                titleCacheRep ?? throw new ArgumentNullException(nameof(titleCacheRep));
        }


        /// <summary>
        /// This reads directly from the distributed cache and makes no attempts to retrieve data from the database on a failed attempt.
        /// </summary>
        /// <param name="clientType"></param>
        /// <param name="appId"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public async Task<TitleVersion> GetTitleAsync(Client clientType, string appId, string alias)
        {
            TitleVersion titleVer = await  _titleCacheRep.GetAppMappingAsync(clientType, appId, alias);
            return titleVer;
        }
    }
}
