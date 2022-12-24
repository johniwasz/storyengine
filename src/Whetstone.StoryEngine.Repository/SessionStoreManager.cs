using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Repository
{
    public class SessionStoreManager : ISessionStoreManager
    {

#pragma warning disable CS1998
        public async Task ClearSessionCacheAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {

            // do nothing
            //storyRequest.SessionContext = null;


        }

#pragma warning disable CS1998
        public async Task<int> GetBadIntentCounterAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {
            EngineSessionContext context = storyRequest?.SessionContext ?? throw new ArgumentNullException($"{nameof(storyRequest)}", "Must have a SessionContext");
            int badIntentCounter = context.BadIntentCounter;
            return badIntentCounter;
        }

#pragma warning disable CS1998
        public async Task<SessionStartType?> GetSessionStartTypeAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {
            EngineSessionContext context = storyRequest?.SessionContext ?? throw new ArgumentNullException($"{nameof(storyRequest)}", "Must have a SessionContext");
            return context.SessionStartType;
        }
#pragma warning disable CS1998
        public async Task<int> IncrementBadIntentCounterAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {
            EngineSessionContext context = storyRequest?.SessionContext ?? throw new ArgumentNullException($"{nameof(storyRequest)}", "Must have a SessionContext");
            context.BadIntentCounter++;

            return context.BadIntentCounter;
        }

#pragma warning disable CS1998
        public async Task ResetBadIntentCounterAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {
            EngineSessionContext context = storyRequest?.SessionContext ?? throw new ArgumentNullException($"{nameof(storyRequest)}", "Must have a SessionContext");
            context.BadIntentCounter = 0;
        }

#pragma warning disable CS1998
        public async Task<SessionStartType> SaveSessionStartTypeAsync(StoryRequest storyRequest)
#pragma warning restore CS1998
        {
            EngineSessionContext context = storyRequest?.SessionContext ?? throw new ArgumentNullException($"{nameof(storyRequest)}", "Must have a SessionContext");
            SessionStartType retType;

            if (storyRequest.IsNewSession.GetValueOrDefault(false))
            {
                retType = string.IsNullOrWhiteSpace(storyRequest.Intent) ? SessionStartType.LaunchStart : SessionStartType.IntentStart;

            }
            else
            {
                retType = SessionStartType.LaunchStart;

            }

            context.SessionStartType = retType;

            return retType;
        }
    }
}
