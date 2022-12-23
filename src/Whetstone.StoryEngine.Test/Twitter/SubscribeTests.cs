using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Data;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Security;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Whetstone.StoryEngine.Security.Claims;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.Test.Twitter
{
    public class SubscribeTests : TestServerFixture
    {
        [Fact]
        public async Task SubscribeWhetstoneIntegrated()
        {
            //ClaimsPrincipal userPrin = await GetClaimsPrincipalAsync();
            ////Get EF context
            //Thread.CurrentPrincipal = userPrin;

            //IOrganizationRepository orgRep = Services.GetService<IOrganizationRepository>();

            //// Get the organization from the current principal.

            //var orgClaim = userPrin.Claims.FirstOrDefault(x => x.Type.Equals(SoniBridgeClaimTypes.Organization));

            //Guid orgId = Guid.Parse(orgClaim.Value as string);

            //var creds = await  orgRep.GetTwitterCredentialsAsync(orgId);

            //TwitterCredentialListItem listItem = creds.FirstOrDefault();


            //await orgRep.SubscribeAsync(orgId, listItem.Id, "localdev");


        }

       
    }
}
