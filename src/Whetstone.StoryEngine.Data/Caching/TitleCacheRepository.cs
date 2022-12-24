using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Caching
{
    public class TitleCacheRepository : ITitleCacheRepository
    {
        private readonly IDistributedCache _distCache;

        private const string CONTAINER_PREFIX = "title";

        private const string TITLE_CONTAINER = "version";

        private const string TITLE_MAPPING = "mapping";


        private readonly IMemoryCache _memCache;

        private readonly IFileReader _fileRep;

        private readonly ILogger<TitleCacheRepository> _titleLogger;

        /// <summary>
        /// Once an item is added to the cache, it needs to updated programmatically or deleted to be refreshed in the cache.
        /// </summary>
        //private readonly DistributedCacheEntryOptions TitleCacheOptions = new DistributedCacheEntryOptions()
        //    {SlidingExpiration = new TimeSpan(100, 0,0,0)};

        // Title Version cache when written to dynamo db do not update with a sliding expiration. Use
        // absolute when writing to dynamo db.
        private readonly DistributedCacheEntryOptions TitleCacheOptions = new DistributedCacheEntryOptions()
        { AbsoluteExpirationRelativeToNow = new TimeSpan(365 * 100, 0, 0, 0) };



        private readonly MemoryCacheEntryOptions InMemoryCacheOptions = new MemoryCacheEntryOptions()
        { AbsoluteExpirationRelativeToNow = new TimeSpan(hours: 0, minutes: 10, 0) };

        public TitleCacheRepository(IFileReader fileRep, IDistributedCache distCache, IMemoryCache memoryCache, ILogger<TitleCacheRepository> logger)
        {
            _distCache = distCache ?? throw new ArgumentNullException(nameof(distCache));

            _memCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));

            _titleLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            _fileRep = fileRep ?? throw new ArgumentNullException(nameof(fileRep));
        }


        public async Task RemoveTitleVersionAsync(TitleVersion titleVersion, bool isPurge)
        {

            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            string titleKey = GetTitleKey(titleVersion);
            _memCache.Remove($"{CONTAINER_PREFIX}:{titleKey}");

            if (isPurge)
                await _distCache.RemoveAsync(CONTAINER_PREFIX, titleKey);
        }


        public async Task<StoryTitle> GetStoryTitleAsync(TitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            string titleKey = GetTitleKey(titleVersion);

            Stopwatch retrievalTime = new Stopwatch();

            string memCacheKey = $"{CONTAINER_PREFIX}:{titleKey}";

            // Check the memory cache first.
            StoryTitle retTitle = _memCache.Get(memCacheKey) as StoryTitle;


            if (retTitle == null)
            {
                //retrievalTime.Restart();
                //retTitle = await _distCache.GetAsync<StoryTitle>(CONTAINER_PREFIX, titleKey);
                //retrievalTime.Stop();


                if (retTitle == null)
                {

                    retrievalTime.Restart();
                    retTitle = await _fileRep.GetTitleContentsAsync(titleVersion);
                    retrievalTime.Stop();


                    if (retTitle != null)
                        _titleLogger.LogDebug($"Title {memCacheKey} returned from file repository: {retrievalTime.ElapsedMilliseconds}");
                }
                else
                {
                    _titleLogger.LogDebug($"Title {memCacheKey} returned from distributed cache: {retrievalTime.ElapsedMilliseconds}");

                }

                if (retTitle != null)
                    _memCache.Set(memCacheKey, retTitle);
            }



            return retTitle;
        }



        public async Task SetTitleVersionAsync(StoryTitle title)
        {
            if (title == null)
                throw new ArgumentNullException(nameof(title));

            string titleKey = GetTitleKey(title);
            await _distCache.SetAsync<StoryTitle>(CONTAINER_PREFIX, titleKey, title, TitleCacheOptions);
            _memCache.Set($"{CONTAINER_PREFIX}:{titleKey}", title);
        }

        public async Task SetAppMappingAsync(Client clientType, string clientAppId, TitleVersion titleVersion)
        {
            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentNullException(nameof(clientAppId));

            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));


            string clientKey = GetClientMappingKey(clientType, clientAppId, titleVersion.Alias);


            _memCache.Set<TitleVersion>($"{CONTAINER_PREFIX}:{clientKey}", titleVersion, InMemoryCacheOptions);
            await _distCache.SetAsync<TitleVersion>(CONTAINER_PREFIX, clientKey, titleVersion, TitleCacheOptions);
        }



        public async Task<TitleVersion> GetAppMappingAsync(Client clientType, string clientAppId, string clientAlias)
        {
            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentNullException(nameof(clientAppId));

            string clientAliasRequest = string.IsNullOrWhiteSpace(clientAlias) ? null : clientAlias;

            string clientKey = GetClientMappingKey(clientType, clientAppId, clientAliasRequest);

            _memCache.TryGetValue<TitleVersion>($"{CONTAINER_PREFIX}:{clientKey}", out var retVersion);

            if (retVersion == null)
            {
                Stopwatch titleVersionTime = new Stopwatch();

                titleVersionTime.Start();
                retVersion = await _distCache.GetAsync<TitleVersion>(CONTAINER_PREFIX, clientKey);
                titleVersionTime.Stop();

                _memCache.Set($"{CONTAINER_PREFIX}:{clientKey}", retVersion, InMemoryCacheOptions);

            }
            else
            {
                _titleLogger.LogDebug(
                    $"title version returned from in memory {CONTAINER_PREFIX} and key {clientKey}");

            }

            return retVersion;

        }

        public async Task RemoveAppMappingAsync(Client clientType, string clientAppId, string alias, bool isPurge)
        {

            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentNullException(nameof(clientAppId));

            string clientKey = GetClientMappingKey(clientType, clientAppId, alias);

            _memCache.Remove($"{CONTAINER_PREFIX}:{clientKey}");

            if (isPurge)
                await _distCache.RemoveAsync(CONTAINER_PREFIX, clientKey);
        }


        private string GetClientMappingKey(Client clientType, string clientAppId, string alias)
        {
            string clientKey;

            string clientTypeText = clientType.ToString().ToLower();

            if (string.IsNullOrWhiteSpace(alias))
            {
                clientKey = $"{TITLE_MAPPING}:{clientTypeText}-{clientAppId}";
            }
            else
                clientKey = $"{TITLE_MAPPING}:{clientTypeText}-{clientAppId}-{alias}";

            return clientKey;
        }

        private string GetTitleKey(TitleVersion titleVer)
        {
            string titleKey = $"{TITLE_CONTAINER}:{titleVer.ShortName}-{titleVer.Version}";

            return titleKey;
        }

        private string GetTitleKey(StoryTitle title)
        {

            string titleKey = $"{TITLE_CONTAINER}:{title.Id}-{title.Version}";

            return titleKey;
        }




    }
}
