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
                "eyJjdHkiOiJKV1QiLCJlbmMiOiJBMjU2R0NNIiwiYWxnIjoiUlNBLU9BRVAifQ.GmkyK3s5TOagEcHBTn6HHsE7l8F9itbo2tydRA9quLQknURcSTKH96YzrAD-VVwGSexYHLoaUQp1M5_3eHVKVSw0eCbui8DMOKT6rySDRZzKWd6XUVn5f7voJvgsqtfm10KP60tNWEDO2xM8TQQ0VSeasACW0xr9CYhY4WYZ-eIcfoGEd-DQ8abLlCbYdJii5zOzLqCNd9ZN0BMVV1GjsGKDKSEWHP5MvgeBe3AhSYAFyqmOGhjDic5G1-ZN0uDuAh3mrSkWW0fbNJrdZdNNhvqUtmXVMVb3_6Bc80hoANsYNTcnr70ka_X6Ubnde7HQLxc0tQW5kmNHedRyaY-FcQ.DE5rIN2SVkKrKN69.bteL442zMK9xj57OMY2kP-i0pNbf4I_Rn9jLQeQTq4bX7Kb6E-CUtzvbv8i0afJ65XmhHWLEEUdZ2dbsHtyJD_8kFz0rFhazSldVzh3atSZyadynZ0589JUUusizWr_CW3PbrK8H2gxhkRy9cY20bRhjfLzYlds9PMqVLT0luwa1OQJ_yH2WNELAtq5qso9YkgKCdPqXGpjjMPlOO_xJrJ8taeZaRPWVXMNaMkaaMsZwwKttWbCvi-Xm7AY84lEtYVZiu-xN8DpZZReTaVwMp4S5fyJdB-dUrXsR2UShUbq2x8vehshHKfdpPmL__CNw-pIY6uqT6T5DVRjIde3NahK03JIdLXnd1mYjCTwgv2F9zFXD57ROB011uxG40a9UoVRqwXciX_ie5Nmvl63Y8SMialgFvZnlSNACzULuUpzmGVHRJFokTJeh4672o2am3aJ5R9KCuk4CG0RHHDUF9JlPNj4_XydFp9KAxEsx-7ppR3xpTxEavfqkIhhca7YcGDstFhzQyEbKGyAjbI6sRCB9aRL1NQG325h8ElrXL9sixKw148mKoo_JiLqe2u43A2Gn1Hn6h_xOibioH1MsjM22vrND4N-JZQOzZM_KQPvFVNuaBrUiQ94fH56jTUXvu8q9-cQgZUA9kBJ7zjcEifPVOsz8MwZU0olCq-vu4socO-uvnLQurxLkEtY_IXGTdsMSCakiDX1ZWINVO9yrvtw22GUzqEJ-GhH_iq0BDIv-ok924r-UTFl9f9waHd7vnxEY8uj8HQv1juMTqqroUNHSfufBIv26DsJP8JvHJbvYiF4x3MFNBu6o-qlgdzy35X1fxPdVTZw2w-TGDNltZeVHUaDAxPyx_n3DZf-1ayajTJfE-egBb1Mg9EuzaSqbMERD63qbnNThrsyygHip-cFm1H6Ql8Nc1DfNdak7-35Z3IzByJncTxLelv1QBXvZdDzCmW6O6jaguhh7jOQCrW6v7UILtSgDgnNuytdhGy3sDJGclKNTlc35uS9TGQMTuDyWfcrj8AZLp2UbyZffnLXaEeGOq_AfCrcCh99LBCD_Fgt4i2OMXNpTwpRuHk543mCrG9nV078vMurzz350zd35QW5Muk-BFCzmhlmTlOXQD3IcvpzuNwBjs7oUE4Fc4z8ZREmnxEjB0enKprJDZgKfy6EdpmrlsAZs56O1JJAhwcoicpmKbdQjvxTbzGlaG4RazssQLQZZXNJS2ZQJImxMn6bjpv7lSR5TWftYbDyEeL4E5Wto5BvxX5jRsxv1LJHOp-00YKu6kRLUTZFOmkzI1qkKOiCgnb3XfJJTp4LWS87OO43Pf2jUx30.DtLW6g0F9lR7hP6OZZdCrg";
            refRequest.AuthToken =

                "eyJraWQiOiJudmllWUdVU2xLUG1yVWppNU9mcEpobWJ6MVVKQ1lrbVB6MXpSb2NxNGFnPSIsImFsZyI6IlJTMjU2In0.eyJzdWIiOiI2N2E3ODZjMC0xNTgxLTQ2OTItYTgyNC05YzcwN2Q0MDZiMGEiLCJldmVudF9pZCI6Ijc0NGNmZGVhLTMxMWQtNGM0Zi1hYjM2LTJjZjc1MWZiODU4OCIsInRva2VuX3VzZSI6ImFjY2VzcyIsInNjb3BlIjoiYXdzLmNvZ25pdG8uc2lnbmluLnVzZXIuYWRtaW4iLCJhdXRoX3RpbWUiOjE1ODg5NjkyODgsImlzcyI6Imh0dHBzOlwvXC9jb2duaXRvLWlkcC51cy1lYXN0LTEuYW1hem9uYXdzLmNvbVwvdXMtZWFzdC0xX3h1MGdlWTJjQyIsImV4cCI6MTU4ODk3Mjg4OCwiaWF0IjoxNTg4OTY5Mjg4LCJqdGkiOiJjMGQ5ZWJjMi0zMTEwLTQyMzAtYmI3Ni02NzNlZTYyNmY1ZDkiLCJjbGllbnRfaWQiOiI0bzI5azE2ZjEwaXIzbzdqb2I4bW1pYWJhZCIsInVzZXJuYW1lIjoiNjdhNzg2YzAtMTU4MS00NjkyLWE4MjQtOWM3MDdkNDA2YjBhIn0.AdB9nMHm8TPz6iHXucAmn47uVegMdJ_lG5LG3dks-w50OjU5dl8IfNOYEKv69-44Qxq782wcdgwy2iNV_4uj-HFGrIg57mplXn22yLy-FWJCj5l3UFEMiMGz9rdab-2MVXouH-Hs0Iu76rZzytPndgRgxDmqKjh8bcRHIaIlZhF2S4chpMwpxc76eWCpNQeI7hmj6panqCkdVEpwfPDKWj8wiXCYuhDEGTMuACHy5UsZ7M8En96SDmr4ASky3qwT8HC9eIvvO4dRE-k5xcr9ZXgmHjC-R1TI0HEM5eTk-xhkVyeSMLOJew19HPWDIkaLW-Rd7LXUBKvhLaWgHCMeXQ";

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
                UserName = "iwaszj@gmail.com",
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
                UserName = "iwaszj@gmail.com",
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
