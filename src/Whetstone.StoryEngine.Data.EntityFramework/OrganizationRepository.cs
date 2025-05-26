using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Cache;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private const string CACHE_CONTAINER = "org";

        private readonly ILogger<OrganizationRepository> _logger;

        private readonly IUserContextRetriever _userContextRetriever;

        private readonly IDistributedCache _distCache;

        private readonly DistributedCacheEntryOptions OrgCacheOptions = new DistributedCacheEntryOptions()
        { SlidingExpiration = new TimeSpan(0, 1, 0, 0) };

        // TODO Use the IAuthorization service to validate project repository access
#pragma warning disable IDE0060 // Remove unused parameter
        public OrganizationRepository(IUserContextRetriever userContextRetriever, IDistributedCache distCache, ILogger<OrganizationRepository> logger)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
    }
}
