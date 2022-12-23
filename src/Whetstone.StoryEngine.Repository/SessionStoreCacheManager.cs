using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Cache;

namespace Whetstone.StoryEngine.Repository
{
    public class SessionStoreCacheManager : ISessionStoreManager
    {
        private const string SESSIONSTARTKEY = "STARTTYPE";

        private const string BADINTENTSKEY = "BADINTENTS";

        private const string CACHEPREFIX = "sessionstore";

        protected IDistributedCache _cache;

        private readonly ILogger<SessionStoreCacheManager> _logger;

        public SessionStoreCacheManager(IDistributedCache cache, ILogger<SessionStoreCacheManager> logger)
        {
            _cache = cache ?? throw new ArgumentNullException($"{nameof(cache)}");

            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }

        #region key builders
        protected string GetSessionStartKey(StoryRequest req)
        {
            string sessionKey = GetSessionKey(req);
            sessionKey = string.Concat(sessionKey, "&", SESSIONSTARTKEY);
            return sessionKey;
        }

        protected string GetBadIntentsKey(StoryRequest req)
        {
            string sessionKey = GetSessionKey(req);
            sessionKey = string.Concat(sessionKey, "&", BADINTENTSKEY);
            return sessionKey;
        }

        protected string GetSessionKey(StoryRequest req)
        {
            string appId = req.ApplicationId;
            string userId = req.UserId;
            string sessionId = req.SessionId;
            Client clientId = req.Client;

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("useId cannot be null or empty when generating the session key");

            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException("appId cannot be null or empty when generating the session key");


            string sessionKey;

            if (string.IsNullOrWhiteSpace(sessionId))
                sessionKey = string.Concat(appId, "&", clientId.ToString(), "&", userId);
            else
                sessionKey = string.Concat(appId, "&", clientId.ToString(), "&", userId, "&", sessionId);

            return sessionKey;
        }


        #endregion


        public virtual async Task<SessionStartType?> GetSessionStartTypeAsync(StoryRequest req)
        {

            if (req == null)
                throw new ArgumentException($"{nameof(req)} cannot be null");

            SessionStartType? retVal;
            string sessionStartKey = GetSessionStartKey(req);

            _logger.LogDebug($"1: Getting Session start key {sessionStartKey}");

            retVal = await _cache.GetAsync<SessionStartType?>(CACHEPREFIX, sessionStartKey);

            if (retVal.HasValue)
            {
                _logger.LogDebug($"2: Session start key {retVal} not found");
            }
            else
                _logger.LogDebug($"2: Session start key {sessionStartKey} value {retVal}");

            return retVal;
        }

        public virtual async Task<SessionStartType> SaveSessionStartTypeAsync(StoryRequest req)
        {
            if (req == null)
                throw new ArgumentException($"{nameof(req)} cannot be null");

            string sessionKey = GetSessionStartKey(req);

            SessionStartType retType = req.RequestType != StoryRequestType.Launch && req.IsNewSession.GetValueOrDefault(false) ?
                SessionStartType.IntentStart :
                SessionStartType.LaunchStart;

            switch (retType)
            {
                case SessionStartType.LaunchStart:
                    _logger.LogInformation($"Session key {sessionKey} started from launch request");
                    break;
                case SessionStartType.IntentStart:
                    _logger.LogInformation($"Session key {sessionKey} started from intent request");
                    break;

            }

            await _cache.SetAsync(CACHEPREFIX, sessionKey, retType);

            _logger.LogDebug($"Session key {sessionKey} saved");

            return retType;
        }

        public async Task<int> GetBadIntentCounterAsync(StoryRequest req)
        {
            string badIntentKey = GetBadIntentsKey(req);


            _logger.LogDebug($"Getting badintentkey {badIntentKey}");

            int retVal = await _cache.GetAsync<int>(CACHEPREFIX, badIntentKey);

            _logger.LogDebug($"Retrieved badintentkey {badIntentKey}, return code: {retVal}");

            return retVal;
        }

        public async Task<int> IncrementBadIntentCounterAsync(StoryRequest req)
        {

            int counter = await GetBadIntentCounterAsync(req);

            counter++;
            await SaveBadIntentCounterAsync(req, counter);

            return counter;
        }

        public async Task ResetBadIntentCounterAsync(StoryRequest req)
        {
            if (req == null)
                throw new ArgumentNullException($"{nameof(req)}");

            await SaveBadIntentCounterAsync(req, 0);
        }

        private async Task SaveBadIntentCounterAsync(StoryRequest req, int counter)
        {

            string counterKey = GetBadIntentsKey(req);

            _logger.LogDebug($"Saving BadIntentCounter {counterKey}, counter: {counter}");

            await _cache.SetAsync<int>(CACHEPREFIX, counterKey, counter);

            _logger.LogDebug($"Saved BadIntentCounter {counterKey}");

        }



        public async Task ClearSessionCacheAsync(StoryRequest req)
        {
            if (req == null)
                throw new ArgumentNullException($"{nameof(req)}");



            string badCounterKey = GetBadIntentsKey(req);

            _logger.LogDebug($"Removing BadIntentCounter {badCounterKey}");

            await _cache.RemoveAsync(CACHEPREFIX, badCounterKey);

            _logger.LogDebug($"Removed BadIntentCounter {badCounterKey}");

            string startTypeKey = GetSessionStartKey(req);

            _logger.LogDebug($"Removing SessionStartKey {startTypeKey}");

            await _cache.RemoveAsync(startTypeKey);

            _logger.LogDebug($"Removed SessionStartKey {startTypeKey}");

        }
    }
}
