using System;
using System.Diagnostics;
using Amazon.Lambda.Core;
using Whetstone.StoryEngine.AlexaProcessor;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Whetstone.StoryEngine.AlexaFunction
{
    // Something to try at some point: Lambda remote debugging:
    // https://medium.com/@zaccharles/remotely-debugging-net-in-aws-lambda-with-breakpoints-88ff57aae1c6

    public class AlexaFunction : AlexaFunctionBase
    {    



    }
}