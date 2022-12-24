using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.Repository
{



    /// <summary>
    /// Manages ephemeral settings that should live only for the duration of the session.
    /// </summary>
    public interface ISessionStoreManager
    {

        /// <summary>
        /// Retrieves the session start type from cache.
        /// </summary>
        /// <remarks>This is used to determine if the user is starting the skill on a valid type.</remarks>
        /// <param name="storyRequest">The request from the speech client.</param>
        /// <returns>Whether the session started from a launch request or from a permitted intent.</returns>
        Task<SessionStartType?> GetSessionStartTypeAsync(StoryRequest storyRequest);

        /// <summary>
        /// Determines the session start type. The type is returned and stored in cache.
        /// </summary>
        /// <param name="storyRequest">The request from the speech client.</param>
        /// <returns>Whether the session started from a launch request or from a permitted intent.</returns>
        Task<SessionStartType> SaveSessionStartTypeAsync(StoryRequest storyRequest);


        /// <summary>
        /// Retrieves the consecutive number of bad intent submissions in a single session for a user.
        /// </summary>
        /// <param name="storyRequest"></param>
        /// <returns>An integer showing the number of consecutive bad intent submissions.</returns>
        Task<int> GetBadIntentCounterAsync(StoryRequest storyRequest);

        /// <summary>
        /// Increments the consecutive number of bad intent submissions by one.
        /// </summary>
        /// <param name="storyRequest"></param>
        /// <returns>The current number of bad intent submissions.</returns>
        Task<int> IncrementBadIntentCounterAsync(StoryRequest storyRequest);

        /// <summary>
        /// Should be run at the start of a session. This resets the number of bad intent counters to zero. 
        /// </summary>
        /// <remarks>
        /// This should be called when the user makes a successful intent request.
        /// </remarks>
        /// <param name="storyRequest"></param>
        Task ResetBadIntentCounterAsync(StoryRequest storyRequest);

        /// <summary>
        /// Remove all values in the cache related to the session.
        /// </summary>
        /// <param name="storyRequest"></param>
        /// <returns></returns>
        Task ClearSessionCacheAsync(StoryRequest storyRequest);
    }
}
