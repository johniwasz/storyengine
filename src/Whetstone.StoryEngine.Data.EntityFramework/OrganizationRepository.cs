
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Twitter;
using Whetstone.StoryEngine.Repository.Twitter;
using Whetstone.StoryEngine.Security.Claims;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class OrganizationRepository :  IOrganizationRepository
    {
        private const string CACHE_CONTAINER = "org";

        private const string CACHE_CONTAINER_TWITCRED = "twitter:cred";

        private readonly ILogger<OrganizationRepository> _logger;


        private readonly IWebHookManager _webHookManager;

        private readonly IUserContextRetriever _userContextRetriever;

        private readonly IDistributedCache _distCache;

        private readonly DistributedCacheEntryOptions OrgCacheOptions = new DistributedCacheEntryOptions()
        { SlidingExpiration = new TimeSpan(0, 1, 0, 0) };

        // TODO Use the IAuthorization service to validate project repository access
#pragma warning disable IDE0060 // Remove unused parameter
        public OrganizationRepository(IUserContextRetriever userContextRetriever, IDistributedCache distCache, IWebHookManager webhookManager, ILogger<OrganizationRepository> logger)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _webHookManager = webhookManager ?? throw new ArgumentNullException(nameof(webhookManager));


            _userContextRetriever = userContextRetriever ?? throw new ArgumentNullException(nameof(userContextRetriever));

            _distCache = distCache ?? throw new ArgumentNullException(nameof(distCache));

        }

        public async Task<IEnumerable<Organization>> GetOrganizationsAsync()
        {

            List<Organization> retOrganizations = null;
            ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

            Guid? userSid = prin?.GetUserSid();

            try
            {
                if (userSid.HasValue)
                {

                    string orgsKey = $"userid:{userSid}";

                    retOrganizations = await _distCache.GetAsync<List<Organization>>(CACHE_CONTAINER, orgsKey);

                    if (retOrganizations == null)
                    {


                        await using var userContext = await _userContextRetriever.GetUserDataContextAsync();

                        var orgsFound = userContext.UserGroupXRefs
                           .Join(userContext.Groups,
                             groupXref => groupXref.GroupId,
                             groups => groups.Id,
                             (groupXref, groups) => new
                             {
                                 UserId = groupXref.UserId,
                                 OrganizationId = groups.OrganizationId
                             })
                           .Join(userContext.Organizations,
                               groupXRef => groupXRef.OrganizationId,
                               org => org.Id,
                               (groupXRef, org) => new
                               {
                                   Organization = org,
                                   UserId = groupXRef.UserId
                               })
                           .Join(userContext.SubscriptionLevels,
                              org => org.Organization.SubscriptionLevelId,
                              subscriptionLevel => subscriptionLevel.Id,
                              (org, subscriptionLevel) => new
                              {
                                  MergedOrganization = org,
                                  SubscriptionLevel = subscriptionLevel.Name
                              }
                           )
                           .Where(x => x.MergedOrganization.UserId.Equals(userSid))
                           .Select(arg =>
                               new
                               {
                                   arg.MergedOrganization.Organization,
                                   arg.SubscriptionLevel
                               });

                        orgsFound.Load();

                        retOrganizations = new List<Organization>();

                        foreach (var dataOrg in orgsFound)
                        {

                            Organization foundOrg = new Organization()
                            {
                                Description = dataOrg.Organization.Description,
                                Name = dataOrg.Organization.Name,
                                Id = dataOrg.Organization.Id,
                                SubscriptionLevel = dataOrg.SubscriptionLevel,
                                IsEnabled = dataOrg.Organization.IsEnabled
                            };

                            retOrganizations.Add(foundOrg);
                        }

                        await _distCache.SetAsync(CACHE_CONTAINER, orgsKey, OrgCacheOptions);
                    }
                }
                else
                {
                    throw new ArgumentException($"Claim {ClaimTypes.Sid} does not contain a valid GUID", nameof(prin));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving organizations for user {userSid}");
                throw;
            }

            return retOrganizations;
        }


        //public async Task<IEnumerable<TwitterCredentialListItem>> GetTwitterCredentialsAsync(Guid organizationId)
        //{

        //    List<TwitterCredentialListItem> twitterCreds = null;

        //    ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

        //    Guid? userSid = prin?.GetUserSid();

        //    Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));

        //    if (orgClaim != null)
        //    {
        //        try
        //        {
                   

        //            await using var userContext = await _userContextRetriever.GetUserDataContextAsync();

        //            var retCreds = userContext.TwitterCredentials.Where(x => x.OrganizationId.Equals(organizationId) && !x.IsDeleted);

        //            retCreds.Load();

        //            twitterCreds = new List<TwitterCredentialListItem>();

        //            foreach (var dataCred in retCreds)
        //            {
        //                TwitterCredentialListItem cred = ToTwitterListItem(dataCred);
        //                twitterCreds.Add(cred);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting twitter credentials for {userSid} and organization id {organizationId}");
        //            throw;
        //        }

        //    }

        //    return twitterCreds;
        //}

        //public async Task<AddTwitterCredentialsResponse> AddTwitterCredentialsAsync(Guid organizationId, AddTwitterCredentialsRequest credRequest)
        //{

        //    AddTwitterCredentialsResponse resp = null;
        //    ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

        //    Guid? userSid = prin?.GetUserSid();
        //    // Validate that the user has a claim for the organization

        //    Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));

        //    if(orgClaim!=null)
        //    {
        //        DataTwitterCredentials dataCreds = new DataTwitterCredentials()
        //        {
        //            Name = credRequest.Name,
        //            AccessToken = credRequest.AccessToken,
        //            AccessTokenSecret = credRequest.AccessTokenSecret,
        //            ConsumerKey = credRequest.ConsumerKey,
        //            ConsumerSecret = credRequest.ComsumerSecret,
        //            BearerToken = credRequest.BearerToken,
        //            Description = credRequest.Description,
        //            OrganizationId = organizationId,
        //            IsDeleted = false,
        //            IsEnabled = true
        //        };

        //        try
        //        {

                  

        //            await using var userContext = await _userContextRetriever.GetUserDataContextAsync();

        //            userContext.TwitterCredentials.Add(dataCreds);

        //            await userContext.SaveChangesAsync();

        //            if (dataCreds.Id.HasValue)
        //            {
        //                resp = new AddTwitterCredentialsResponse()
        //                {
        //                    Id = dataCreds.Id.Value
        //                };
        //            }

        //            string credKey = GetTwitterCredentialKey(organizationId, resp.Id);

        //            TwitterCredentialListItem listItem = ToTwitterListItem(dataCreds);

        //           await _distCache.SetAsync(CACHE_CONTAINER_TWITCRED, credKey, listItem, OrgCacheOptions);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error adding twitter credentials for {userSid} and organization id {organizationId}");
        //            throw;
        //        }
        //    }

        //    return resp;
        //}


        //public async Task<IEnumerable<TwitterWebhookListItem>> GetTwitterWebhooksAsync(Guid organizationId, Guid credentialsId, string environment)
        //{
        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));

        //    IEnumerable<TwitterWebhookListItem> webhooks = null;


        //    Claim orgClaim = GetUserSidClaim(organizationId);

        //    if (orgClaim != null)
        //    {
              
        //        try
        //        {
        //            TwitterCredentialListItem creds = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //            webhooks = await _webHookManager.GetWebHooksAsync(creds, environment);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting webhooks for {orgClaim.Value} and organization id {organizationId} in envrionment {environment}");
        //            throw;

        //        }
        //    }

        //    return webhooks;
        //}

        //public async Task SubscribeAsync(Guid organizationId, Guid credentialsId, string environment)
        //{

        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));


        //    Claim orgClaim = GetUserSidClaim(organizationId);

        //    if (orgClaim != null)
        //    {

        //        try
        //        {
        //            TwitterCredentialListItem creds = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //            await _webHookManager.SubscribeAsync(creds, environment);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error subscribing to activities for {orgClaim.Value} and organization id {organizationId} in envrionment {environment}");
        //            throw;

        //        }
        //    }

        //}

        //public async Task UnsubscribeAsync(Guid organizationId, Guid credentialsId, string environment, long userId)
        //{

        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));


        //    Claim orgClaim = GetUserSidClaim(organizationId);

        //    if (orgClaim != null)
        //    {

        //        try
        //        {
        //            TwitterCredentialListItem creds = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //            await _webHookManager.UnsubscribeAsync(creds, environment, userId);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error unsubscribing from activities for {orgClaim.Value} and organization id {organizationId} in envrionment {environment}");
        //            throw;

        //        }
        //    }

        //}


        //public async Task DeleteTwitterWebhookAsync(Guid organizationId, Guid credentialsId, string environment, string webhookId)
        //{

        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));

        //    if (string.IsNullOrWhiteSpace(webhookId))
        //        throw new ArgumentNullException(nameof(webhookId));

        //    TwitterCredentialListItem credItem = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //    ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

        //    Guid? userSid = prin?.GetUserSid();

        //    Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));


        //    if (orgClaim != null)
        //    {

        //        try
        //        {
        //            TwitterCredentialListItem creds = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //            await _webHookManager.DeleteWebhookAsync(creds, environment, webhookId);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error deleting webbook {webhookId} for {userSid} and organization id {organizationId} in environment {environment}");
        //            throw;

        //        }
        //    }
        //}


        //public async Task<TwitterWebhookListItem> RegisterTwitterWebhookAsync(Guid organizationId, Guid credentialsId, string environment, Uri url)
        //{
        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));

        //    if (url == null)
        //        throw new ArgumentNullException(nameof(url));

        //    TwitterWebhookListItem retItem = null;

        //    TwitterCredentialListItem credItem = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //    ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

        //    Guid? userSid = prin?.GetUserSid();

        //    Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));
           
        //    if (orgClaim != null)
        //    {

        //        try
        //        {
        //            TwitterCredentialListItem creds = await GetTwitterCredentialAsync(organizationId, credentialsId);

        //            var param = new Dictionary<string, string>() { { "orgid", organizationId.ToString() },
        //            { "credid", credentialsId.ToString() }};

        //            var newUrl = new Uri(QueryHelpers.AddQueryString(url.AbsoluteUri, param));                    
                  

        //            retItem = await _webHookManager.RegisterWebhookAsync(creds, environment, newUrl);

        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error registering webbook {url} for {userSid} and organization id {organizationId} in environment {environment}");
        //            throw;

        //        }


        //    }

        //    return retItem;
        //}

        //public async Task<TwitterCredentialListItem> GetTwitterCredentialAsync(Guid organizationId, Guid credentialId, bool? bypassSecurity = true)
        //{

        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialId == null)
        //        throw new ArgumentNullException(nameof(credentialId));


        //    Claim orgClaim = null;
        //    Guid? userSid = null;

        //    TwitterCredentialListItem  retCred = null;

        //    if (!bypassSecurity.GetValueOrDefault(false))
        //    {
        //        orgClaim = GetUserSidClaim(organizationId);
        //    }


        //    if (orgClaim != null || bypassSecurity.GetValueOrDefault(false))
        //    {
        //        try
        //        {

        //            string credKey = GetTwitterCredentialKey(organizationId, credentialId);

        //            retCred = await _distCache.GetAsync<TwitterCredentialListItem>(CACHE_CONTAINER_TWITCRED, credKey);

        //            if (retCred == null)
        //            {
        //                await using var userContext = await _userContextRetriever.GetUserDataContextAsync();

        //                var foundCred = userContext.TwitterCredentials.FirstOrDefault(x => x.OrganizationId.Equals(organizationId) && !x.IsDeleted && x.Id.Equals(credentialId));

        //                if (foundCred != null)
        //                {
        //                    retCred = ToTwitterListItem(foundCred);

        //                }

        //                await _distCache.SetAsync(CACHE_CONTAINER_TWITCRED, credKey, retCred, OrgCacheOptions);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting twitter credential {credentialId} for {userSid} and organization id {organizationId}");
        //            throw;
        //        }

        //    }

        //    return retCred;
        //}

        //public async Task DeleteTwitterCredentialsAsync(Guid organizationId, Guid credentialsId)
        //{
        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialsId == null)
        //        throw new ArgumentNullException(nameof(credentialsId));
        

        //    ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

        //    Guid? userSid = prin?.GetUserSid();

        //    Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));

        //    if (orgClaim != null)
        //    {
        //        try
        //        {
        //            string credKey = GetTwitterCredentialKey(organizationId, credentialsId);

        //            await _distCache.RemoveAsync(CACHE_CONTAINER_TWITCRED, credKey);

        //            await using var userContext = await _userContextRetriever.GetUserDataContextAsync();
        //            var foundCred = userContext.TwitterCredentials.FirstOrDefault(x => x.OrganizationId.Equals(organizationId) && !x.IsDeleted && x.Id.Equals(credentialsId));

        //            if (foundCred != null)
        //            {
        //                foundCred.IsDeleted = true;
        //                await userContext.SaveChangesAsync();
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting deleting credential {credentialsId} for {userSid} and organization id {organizationId}");
        //            throw;
        //        }
        //    }
        //}

        //public async Task<GetSubscriptionsResponse> GetTwitterSubscriptionsAsync(Guid organizationId, Guid credentialId, string environment)
        //{
        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialId == null)
        //        throw new ArgumentNullException(nameof(credentialId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));


        //    GetSubscriptionsResponse subResp = null;

        //    Claim orgClaim = GetUserSidClaim(organizationId);

        //    if (orgClaim != null)
        //    {
        //        try
        //        {
        //            var foundCred = await GetTwitterCredentialAsync(organizationId, credentialId);

        //            if (foundCred != null)
        //            {
        //                subResp = await _webHookManager.GetSubscriptionsAsync(foundCred, environment);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting getting twitter subscriptions using {credentialId} for {orgClaim.Value} and organization id {organizationId} for environment {environment}");
        //            throw;
        //        }
        //    }

        //    return subResp;
        //}

        //public async Task ResendWebhookValidationAsync(Guid organizationId, Guid credentialId, string environment, string webhookId)
        //{
        //    if (organizationId == null)
        //        throw new ArgumentNullException(nameof(organizationId));

        //    if (credentialId == null)
        //        throw new ArgumentNullException(nameof(credentialId));

        //    if (string.IsNullOrWhiteSpace(environment))
        //        throw new ArgumentNullException(nameof(environment));

        //    if (string.IsNullOrWhiteSpace(webhookId))
        //        throw new ArgumentNullException(nameof(webhookId));


        //    Claim orgClaim = GetUserSidClaim(organizationId);

        //    if (orgClaim != null)
        //    {
        //        try
        //        {
        //            var foundCred = await GetTwitterCredentialAsync(organizationId, credentialId);

        //            if (foundCred != null)
        //            {
        //                await _webHookManager.ResendWebhookValidationAsync(foundCred, environment, webhookId);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, $"Error getting getting twitter subscriptions using {credentialId} for {orgClaim.Value} and organization id {organizationId} for environment {environment}");
        //            throw;
        //        }
        //    }
        //}

        private Claim GetUserSidClaim(Guid organizationId)
        {

            ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

            Guid? userSid = prin?.GetUserSid();

            Claim orgClaim = prin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization) && Guid.Parse(x.Value).Equals(organizationId));

            return orgClaim;
        }

        private static TwitterCredentialListItem ToTwitterListItem(DataTwitterCredentials foundCred)
        {
            return new TwitterCredentialListItem()
            {
                Id = foundCred.Id.Value,
                Name = foundCred.Name,
                Description = foundCred.Description,
                AccessToken = foundCred.AccessToken,
                AccessTokenSecret = foundCred.AccessTokenSecret,
                BearerToken = foundCred.BearerToken,
                ConsumerSecret = foundCred.ConsumerSecret,
                ConsumerKey = foundCred.ConsumerKey,
                IsEnabled = foundCred.IsEnabled
            };
        }

        private string GetTwitterCredentialKey(Guid organizationId, Guid credentialId)
        {

            string credKey = $"org:{organizationId}:cred:{credentialId}";
            return credKey;
        }
    }
}
