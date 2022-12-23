using Amazon.DynamoDBv2;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;

namespace Whetstone.StoryEngine.Repository.Twitter
{
    public class TwitterApplicationManager : ITwitterApplicationManager
    {
        private readonly ILogger<TwitterApplicationManager> _logger;


        private readonly IWebHookManager _webHookManager;

        private readonly IUserContextRetriever _userContextRetriever;

        private readonly IDistributedCache _distCache;

        private readonly DistributedCacheEntryOptions OrgCacheOptions = new DistributedCacheEntryOptions()
        { SlidingExpiration = new TimeSpan(0, 1, 0, 0) };

        public TwitterApplicationManager(IUserContextRetriever userContextRetriever, IDistributedCache distCache, IWebHookManager webhookManager, ILogger<TwitterApplicationManager> logger)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _webHookManager = webhookManager ?? throw new ArgumentNullException(nameof(webhookManager));

            _userContextRetriever = userContextRetriever ?? throw new ArgumentNullException(nameof(userContextRetriever));

            _distCache = distCache ?? throw new ArgumentNullException(nameof(distCache));

        }


        public async Task<AddTwitterApplicationResponse> AddTwitterApplicationAsync(AddTwitterApplicationRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Credentials == null)
                throw new ArgumentNullException(nameof(request.Credentials));


            AddTwitterApplicationResponse appResponse = null;


            DataTwitterApplication newTwitterApp = new DataTwitterApplication();

            if (request.Credentials != null)
            {
                newTwitterApp.TwitterCredentials = new DataTwitterCredentials
                {
                    AccessToken = request.Credentials.AccessToken,
                    BearerToken = request.Credentials.BearerToken,
                    AccessTokenSecret = request.Credentials.AccessTokenSecret,
                    ConsumerKey = request.Credentials.ConsumerKey,
                    ConsumerSecret = request.Credentials.ConsumerSecret
                };
            }

            using (var context = await _userContextRetriever.GetUserDataContextAsync())
            {
                context.TwitterApplications.Add(newTwitterApp);
                await context.SaveChangesAsync();
            }



            return appResponse;
        }
    }
}
