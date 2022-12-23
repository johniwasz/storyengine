using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito.Extensions;
using Amazon.CognitoIdentityProvider;
using Google.Apis.Logging;
using Microsoft.Extensions.Logging;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.Security.Amazon;
using Xunit;
using Microsoft.Extensions.Caching.Distributed;


namespace Whetstone.StoryEngine.Test.Security
{
    public class CognitoAuthTests : TestServerFixture
    {

        [Fact]
        public async Task AuthTestAsync()
        {


            ILogger<CognitoAuthenticator> logger = CreateLogger<CognitoAuthenticator>();

            CognitoConfig clientOpts = new CognitoConfig();
            clientOpts.UserPoolClientId = "4o29k16f10ir3o7job8mmiabad";
            clientOpts.UserPoolClientSecret = "u2umpl74d54vmf5jglhun1jg7r31ktuehvtlnl80o03m2kvk4nq";
            clientOpts.UserPoolId = "us-east-1_xu0geY2cC";


            
            IAmazonCognitoIdentityProvider cogProvider = new AmazonCognitoIdentityProviderClient();

            //DescribeIdentityProviderRequest poolReq = new DescribeIdentityProviderRequest();
            //poolReq.UserPoolId = clientOpts.UserPoolId;

            // var descResp = await cogProvider.DescribeIdentityProviderAsync(poolReq);

            DescribeUserPoolClientRequest clientReq = new DescribeUserPoolClientRequest();
            clientReq.ClientId = clientOpts.UserPoolClientId;
            clientReq.UserPoolId = clientOpts.UserPoolId;

            var clientInfo = await cogProvider.DescribeUserPoolClientAsync(clientReq);


            DescribeUserPoolRequest idPoolDesc = new DescribeUserPoolRequest();

            idPoolDesc.UserPoolId = clientOpts.UserPoolId;

            var poolInfo = await cogProvider.DescribeUserPoolAsync(idPoolDesc);

            DescribeUserPoolDomainRequest domRequest = new DescribeUserPoolDomainRequest();

            domRequest.Domain = "sonibridge-dev";

            var domResponse = await cogProvider.DescribeUserPoolDomainAsync(domRequest);



            IOptions<CognitoConfig> cogOpts = Options.Create<CognitoConfig>(clientOpts);

            IUserContextRetriever dataContext = this.GetLocalUserContext();


            ILogger<CognitoTokenParser> tokenParserLogger = CreateLogger<CognitoTokenParser>();
            IJwtTokenParser jwtParser = new CognitoTokenParser(cogOpts, tokenParserLogger);

            IDistributedCache cache = this.GetMemoryCache();


            IAuthenticator authenticator = new CognitoAuthenticator(cogProvider, dataContext, cogOpts, jwtParser, cache, logger);


            IAuthenticator auth = GetCognitoAuthenticator();

            AuthCredentials testCreds = await GetTestCredentialsAsync(TESTCRED01);

            var authResults  = await authenticator.AuthenticateAsync(testCreds);



            // Attempt a refresh
            RefreshSessionRequest refreshReq = new RefreshSessionRequest();
            refreshReq.RefreshToken = authResults.RefreshToken;
            refreshReq.AuthToken = authResults.AuthToken;
           // refreshReq.SystemUserId = authResults.

            authResults = await authenticator.RefreshTokenAsync(refreshReq);


            SignOutRequest signOutReq = new SignOutRequest
            {
                AuthToken = authResults.AuthToken
            };
            await authenticator.SignOutAsync(signOutReq);


        }
    }
}
