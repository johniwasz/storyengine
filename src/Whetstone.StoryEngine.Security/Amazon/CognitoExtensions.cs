using System;
using System.Collections.Generic;
using System.Text;
using Amazon.AspNetCore.Identity.Cognito.Extensions;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.Security.Amazon
{
    public static class CognitoExtensions
    {

        public static AWSCognitoClientOptions ToAwsCognitoClientOptions(this CognitoConfig config)
        {


            AWSCognitoClientOptions awsOpts = new AWSCognitoClientOptions
            {
                UserPoolClientSecret = config.UserPoolClientSecret,
                UserPoolClientId = config.UserPoolClientId,
                UserPoolId = config.UserPoolId
            };

            return awsOpts;
        }


    }
}
