using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.Runtime;
using Whetstone.StoryEngine;
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
