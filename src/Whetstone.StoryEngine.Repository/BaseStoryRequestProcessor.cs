using Amazon;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public abstract class BaseStoryRequestProcessor : IStoryRequestProcessor
    {
        protected readonly RegionEndpoint _endpoint;
        private readonly ILogger _logger;

        public BaseStoryRequestProcessor(Func<UserRepositoryType, IStoryUserRepository> userRepFunc, ISessionStoreManager sessionStoreManager, IOptions<EnvironmentConfig> envOptions, ILogger logger)
        {
            if (envOptions == null)
                throw new ArgumentNullException(nameof(envOptions));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            SessionStoreManager = sessionStoreManager ?? throw new ArgumentNullException(nameof(sessionStoreManager));

            if (userRepFunc == null)
                throw new ArgumentNullException(nameof(userRepFunc));

            IStoryUserRepository userRep = userRepFunc(UserRepositoryType.DynamoDB);

            StoryUserRepository = userRep ?? throw new ArgumentNullException(nameof(userRep));
            _endpoint = envOptions.Value.Region;
        }

        public abstract Task<StoryResponse> ProcessStoryRequestAsync(StoryRequest request);

        public abstract Task<CanFulfillResponse> CanFulfillIntentAsync(StoryRequest request);

        public abstract Task<StoryPhoneInfo> GetPhoneInfoAsync(TitleVersion titleVersion);

        protected IStoryUserRepository StoryUserRepository
        {
            get; set;
        }


        protected ISessionStoreManager SessionStoreManager
        {
            get; set;
        }


        /// <summary>
        /// Retrieves the current user
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected async Task<DataTitleClientUser> GetCurrentUser(StoryRequest request)
        {
            Stopwatch userRetrievalTime = Stopwatch.StartNew();

            DataTitleClientUser curUser = null;


            try
            {
                // Gin up a user object if this is a Ping Request so we don't add the object
                // to the database
                if (!request.IsPingRequest.GetValueOrDefault(false))
                {

                    // If the user is a guest user, then the user id will only be applicable during this session
                    // use the session id as the user id.
                    if (request.IsGuest.GetValueOrDefault(false) && string.IsNullOrWhiteSpace(request.UserId))
                        request.UserId = request.SessionId;


                    // if the user id is not provided, then assign one.
                    curUser = await StoryUserRepository.GetUserAsync(request);

                    if (string.IsNullOrWhiteSpace(request.UserId))
                    {
                        request.UserId = curUser.Id.ToString();
                    }
                }
                else
                {
                    // Bootstrap user creates a faux user account without saving the user to the database.
                    _logger.LogInformation($"Bootstrapped Ping Request User {request.UserId} starting the skill.");
                    curUser = StoryUserRepository.BootstrapUser(request);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ErrorEvents.UserRetrievalError,
                                       ex,
                                       $"Error retrieving user {request.UserId}",
                                       request.Client);
            }

            // Assign the current user id to the session context. This is used downstream.

            if (curUser?.Id.GetValueOrDefault() != default(Guid))
                request.SessionContext.EngineUserId = curUser?.Id;


            _logger.LogInformation($"Time to retrieve user: {userRetrievalTime.ElapsedMilliseconds} ms");

            return curUser;
        }






    }
}
