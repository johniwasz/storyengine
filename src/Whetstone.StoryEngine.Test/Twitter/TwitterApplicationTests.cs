using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Twitter;
using Whetstone.StoryEngine.Repository.Twitter;
using Xunit;

namespace Whetstone.StoryEngine.Test.Twitter
{
    public class TwitterApplicationTests : TestServerFixture
    {

        [Fact]
        public async Task AddValidTwitterApplication()
        {

            ClaimsPrincipal userPrin = await GetClaimsPrincipalAsync();
            //Get EF context
            Thread.CurrentPrincipal = userPrin;

            TwitterCredentials creds = await GetSecretAsync<TwitterCredentials>("dev/twitterapp/whetstonecreds");

            AddTwitterApplicationRequest addTwitterApp = new AddTwitterApplicationRequest();

            addTwitterApp.Name = "Whetstone Bot";
            addTwitterApp.IsEnabled = true;
            addTwitterApp.Description = "Whetstone test bot app";
            addTwitterApp.Credentials = new AddTwitterApplicationCredentialsRequest
            {
                AccessToken = creds.AccessToken,
                AccessTokenSecret = creds.AccessTokenSecret,
                BearerToken = creds.BearerToken,
                ConsumerKey = creds.ConsumerKey,
                ConsumerSecret = creds.ComsumerSecret
            };

            ITwitterApplicationManager appManager = this.Services.GetRequiredService<ITwitterApplicationManager>();

            var appResponse = await appManager.AddTwitterApplicationAsync(addTwitterApp);



        }

    }
}
