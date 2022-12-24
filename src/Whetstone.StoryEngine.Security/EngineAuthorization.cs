using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;

namespace Whetstone.StoryEngine.Security
{
    public class EngineAuthorization : IAuthorizationService
    {

        // private IUserContextRetriever _userContextRetriever;

        private readonly ILogger<EngineAuthorization> _logger;

#pragma warning disable IDE0060 // Remove unused parameter
        public EngineAuthorization(IUserContextRetriever contextRetriever, ILogger<EngineAuthorization> logger)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            // _userContextRetriever = contextRetriever ?? throw new ArgumentNullException(nameof(contextRetriever));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource,
            IEnumerable<IAuthorizationRequirement> requirements)
        {

            return await Task.Run(() =>
            {
                AuthorizationResult authResult = null;



                List<IAuthorizationRequirement> failedRequirements = new List<IAuthorizationRequirement>();

                foreach (IAuthorizationRequirement authReq in requirements)
                {

                    if (authReq is ClaimsAuthorizationRequirement permReq)
                    {

                        Claim foundClaim = null;

                        if (permReq.AllowedValues == null)
                        {
                            foundClaim = user.Claims.FirstOrDefault(x => x.Type.Equals(permReq.ClaimType));

                        }
                        else
                        {
                            foundClaim = user.Claims.FirstOrDefault(x => x.Type.Equals(permReq.ClaimType)
                                                                       && permReq.AllowedValues.Contains(x.Value));

                        }


                        // ReSharper disable once InvertIf
                        if (foundClaim == null)
                        {
                            failedRequirements.Add(authReq);
                            if (permReq.AllowedValues != null)
                            {
                                string permissions = string.Join(",", permReq.AllowedValues);
                                _logger.LogWarning(
                                    $"User requested a denied claim {permReq.ClaimType} with value(s) {permissions}");
                            }
                            else
                            {
                                _logger.LogWarning(
                                    $"User requested claim {permReq.ClaimType} that was not found.");
                            }
                        }

                    }

                }

                authResult = failedRequirements.Any()
                    ? AuthorizationResult.Failed(AuthorizationFailure.Failed(failedRequirements))
                    : AuthorizationResult.Success();


                return authResult;
            });
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
        {
            var tcs = new TaskCompletionSource<AuthorizationResult>();
            tcs.SetException(new NotImplementedException());
            return tcs.Task;
        }
    }
}
