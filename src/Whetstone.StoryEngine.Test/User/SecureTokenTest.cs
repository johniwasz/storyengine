using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using System.Threading.Tasks;
using Xunit;

namespace Whetstone.StoryEngine.Test.User
{


    public class SecureTokenTest
    {

        [Fact]
        public async Task GetTokenAsync()
        {
            // AWSCredentials creds = new AWSCredentials();

            using (AmazonSecurityTokenServiceClient secClient = new AmazonSecurityTokenServiceClient(Amazon.RegionEndpoint.USEast1))
            {

                GetSessionTokenRequest tokReq = new GetSessionTokenRequest();

                // tokReq.

                var sessionTokenResp = await secClient.GetSessionTokenAsync();

                string sessionToken = sessionTokenResp.Credentials.SessionToken;


            }




        }

    }
}
