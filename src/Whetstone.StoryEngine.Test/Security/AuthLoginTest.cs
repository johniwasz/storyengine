using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Amazon.AspNetCore.Identity.Cognito.Extensions;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Security;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Security.Amazon;
using SignUpRequest = Whetstone.StoryEngine.Security.SignUpRequest;
using Microsoft.Extensions.Caching.Distributed;
using Tweetinvi.Core.RateLimit;

namespace Whetstone.StoryEngine.Test.Security
{
    public class AuthLoginTest : TestServerFixture
    {


        [Fact]
        public async Task JwtValidationTestAsync()
        {

            IAuthenticator auth = GetCognitoAuthenticator();

            AuthCredentials testCreds = await GetTestCredentialsAsync(TESTCRED01);


            var authTokenInfo =  await auth.AuthenticateAsync(testCreds);


            IJwtTokenParser tokAuth = GetCognitoTokenParser();

            var jwtToken = tokAuth.ParseAuthToken(authTokenInfo.AuthToken, true);

        }

        [Fact]
        public async Task RefreshTokenTestAsync()
        {

            IAuthenticator auth = GetCognitoAuthenticator();


            AuthCredentials testCreds = await GetTestCredentialsAsync(TESTCRED01);
            var authTokenInfo = await auth.AuthenticateAsync(testCreds);


            RefreshSessionRequest refRequest = new RefreshSessionRequest();
            refRequest.RefreshToken = authTokenInfo.RefreshToken;
            refRequest.AuthToken = authTokenInfo.AuthToken;


           TokenResult result = await auth.RefreshTokenAsync(refRequest);
        }


        [Fact]
        public async Task RefreshRawTokenTestAsync()
        {


            RefreshSessionRequest refRequest = new RefreshSessionRequest();
            refRequest.RefreshToken =
                "XXXXX";
            refRequest.AuthToken =

                "XXXX";

            IAuthenticator auth = GetCognitoAuthenticator();

            TokenResult result = await auth.RefreshTokenAsync(refRequest);
        }


        [Fact]
        public async Task AddNewCognitoUserAsync()
        {
           var dataContext = this.GetLocalUserContext();

            DataUser newCogUser = new DataUser
            {
                CognitoSub = "f63428a4-ff20-4fd4-9adc-5329872e063d"
            };

            using (var context = await dataContext.GetUserDataContextAsync())
            {
                await context.CreateUserAccountAsync(newCogUser, "jiwasz@yahoo.com");
            }


        }

        [Fact]
        public async Task CreateNewAccountTestAsync()
        {
            IAuthenticator auth = GetCognitoAuthenticator();

            SignUpRequest newRequest = new SignUpRequest
            {
                UserName = "jiwasz@yahoo.com",
                UserSecret = "M!something3523"
            };

            bool doesUserExist = false;

            try
            {
                await auth.SignUpAsync(newRequest);
            }
            catch (UnexpectedSecurityException e)
            {
                if (e.InnerException is UsernameExistsException)
                    doesUserExist = true;
                else
                    throw;
            }

            if (doesUserExist)
            {
                ConfirmationCodeResendRequest resendRequest = new ConfirmationCodeResendRequest
                {
                    UserName = newRequest.UserName
                };

                await auth.RequestNewConfirmationCode(resendRequest);
            }
           
        }




        [Fact]
        public async Task ConfirmNewAccountTestAsync()
        {

            IAuthenticator auth = GetCognitoAuthenticator();

            ConfirmNewUserRequest confirmUser = new ConfirmNewUserRequest()
            {
                UserName = "someuser@gmail.com",
                ConfirmationCode = "277315"
            };
          var result =  await auth.ConfirmAccountAsync(confirmUser);
        }


        [Fact]
        public async Task RequestNewConfirmationCodeBadAccountTestAsync()
        {

            IAuthenticator auth = GetCognitoAuthenticator();

            ConfirmationCodeResendRequest resendCodeRequest = new ConfirmationCodeResendRequest()
            {
                UserName = "thisuser@doesnotexist.com"
            };
           await auth.RequestNewConfirmationCode(resendCodeRequest);
        }


        [Fact]
        public async Task RequestConfirmationCodeResendAsync()
        {

            IAuthenticator auth = GetCognitoAuthenticator();

            ConfirmationCodeResendRequest confirmUser = new ConfirmationCodeResendRequest()
            {
                UserName = "someuser@gmail.com",
            };
            await auth.RequestNewConfirmationCode(confirmUser);
        }




        private IJwtTokenParser GetCognitoTokenParser()
        {
            IAmazonCognitoIdentityProvider cogProvider = new AmazonCognitoIdentityProviderClient();

            IOptions<CognitoConfig> cogOpts = GetCognitoConfig();


            ILogger<CognitoTokenParser> logger = CreateLogger<CognitoTokenParser>();

            //  IUserDataContext dataContext = this.Services.GetService<IUserDataContext>();

            IUserContextRetriever dataContext = this.GetLocalUserContext();

            IJwtTokenParser auth = new CognitoTokenParser(cogOpts, logger);

            return auth;


        }

        private IAuthenticator GetCognitoTokenAuthenticator()
        {
            IAmazonCognitoIdentityProvider cogProvider = new AmazonCognitoIdentityProviderClient();

            IOptions<CognitoConfig> cogOpts = GetCognitoTokenConfig();


            ILogger<CognitoAuthenticator> logger = CreateLogger<CognitoAuthenticator>();

            IUserContextRetriever dataContext = this.GetLocalUserContext();

            ILogger<CognitoTokenParser> cogTokenLogger = CreateLogger<CognitoTokenParser>();


            IJwtTokenParser tokenParser  = new CognitoTokenParser(cogOpts, cogTokenLogger);

            IDistributedCache cache = this.Services.GetService<IDistributedCache>();


            IAuthenticator auth = new CognitoAuthenticator(cogProvider, dataContext, cogOpts, tokenParser, cache, logger);

            return auth;


        }




        private IOptions<CognitoConfig> GetCognitoTokenConfig()
        {
            CognitoConfig clientOpts = new CognitoConfig
            {
                UserPoolClientId = "4o29k16f10ir3o7job8mmiabad",
                UserPoolId = "us-east-1_xu0geY2cC"
            };
            IOptions<CognitoConfig> cogOpts = Options.Create<CognitoConfig>(clientOpts);

            return cogOpts;

        }
    }
}
