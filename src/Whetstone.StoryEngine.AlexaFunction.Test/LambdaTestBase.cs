using Amazon;
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.DependencyInjection;
using Whetstone.StoryEngine.Test;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public abstract class LambdaTestBase : TestServerFixture
    {

        internal const string InsuranceAgentDevSkillId = "amzn1.ask.skill.ae3930f5-f28e-474a-9735-2d1490491702";

        internal const string ElderGodsSkilId = "amzn1.ask.skill.94c3fd9c-a915-48d8-998b-b96f85604409";


        protected const string JohnId = "amzn1.ask.account.AHFCMHMLDLYOWUSFGZISBIEFY67AEXMXKC2WYDAJ6D3VZADVIHPNXEXPRG6JMMLF3YWWIN7Z5YZDZVKFPTP3XSGIDDSFTYPZ5QYZU4N7XFBKACWSQIRXIPRE2YYZNV2YRPKHSY5MCE5VNCPYZMTSYASF6DFN4OMHTBVE3GC5LHISZ7GZR5Q3F4YGZIO47F7C35QV4FNVVIV4HUQ";

        protected const string JohnSessionId = "amzn1.ask.account.AHFCMHMLDLYOWUSFGZISBIEFY67AEXMXKC2WYDAJ6D3VZADVIHPNXEXPRG6JMMLF3YWWIN7Z5YZDZVKFPTP3XSGIDDSFTYPZ5QYZU4N7XFBKACWSQIRXIPRE2YYZNV2YRPKHSY5MCE5VNCPYZMTSYASF6DFN4OMHTBVE3GC5LHISZ7GZR5Q3F4YGZIO47F7C35QV4FNVVIV4HDR";

        protected const string JohnTestId = "amzn1.ask.account.c55e63f1-b638-41b2-bb03-d040567aa1d9";

        protected const string JohnWhetstoneId = "amzn1.ask.account.AGXP5UXVVOA3A62CF2FQXBYRE77TL2POQNU6IHZ4OTI452KS3X6ZDJ44ZUVHNIAS46RWBMQDBPQYXT2STIBPRJMGP42XPEZVTVBXULTBLDBQAQWHF4H2LUHSVHVNSU7M6TLSY6RFBLZS2VUMECKTPIYYYAS4M6BEGRWPH7AJ5LTHJLHVEZQ4Q6KMQBQF63YYCLJ3GFUTKHK7VFQ";


        public LambdaTestBase()
        {
            Environment.SetEnvironmentVariable(ClientLambdaBase.STORYBUCKETCONFIG,
                "whetstonebucket-dev-s3bucket-1nridm382p5vm");


            Environment.SetEnvironmentVariable(ClientLambdaBase.CACHESLIDINGCONFIG,
                "900");

            Environment.SetEnvironmentVariable(ClientLambdaBase.CACHETABLECONFIG,
                "Whetstone-CacheTable-Dev-CacheTable-1A0X189QJZXYD");

            Environment.SetEnvironmentVariable(ClientLambdaBase.USERTABLECONFIG,
                "Whetstone-DynamoDbStore-Dev-UserTable-1U8IU6T4JWRFU");

            Environment.SetEnvironmentVariable(ClientLambdaBase.MESSAGESTEPFUNCTIONCONFIG,
                "arn:aws:states:us-east-1:940085449815:stateMachine:MessageSenderStateMachine-TatdcODo5DL1");

            Environment.SetEnvironmentVariable(ClientLambdaBase.SESSIONAUDITURLCONFIG,
                "https://sqs.us-east-1.amazonaws.com/940085449815/WhetstoneQueue-Dev-SessionAuditQueue-ZQNCFM1I9XSL");


            Environment.SetEnvironmentVariable("AWS_XRAY_CONTEXT_MISSING", "LOG_ERROR");

        }


        protected TestLambdaContext GetLambdaContext()
        {



            return GetLambdaContext(null);


        }

        protected TestLambdaContext GetLambdaContext(string localStorePath)
        {
            var context = new TestLambdaContext();

            TestClientContext testContext = new TestClientContext();

            context.Logger = new DebugLogger();
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.AWSDEFAULTREGION, RegionEndpoint.USEast1.SystemName);
            System.Environment.SetEnvironmentVariable(ContainerSettingsReader.BOOTSTRAP, "/storyengine/dev/bootstrap");


            context.ClientContext = testContext;
            return context;

        }

        protected async Task<AlexaResponse> SendIntent(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent, Dictionary<string, dynamic> sessionAttribs)
        {

            return await SendIntent(sessionContext, function, intent, null, RequestType.IntentRequest, sessionAttribs);

        }


        protected async Task<AlexaResponse> SendIntent(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent, Dictionary<string, string> slotValues, RequestType reqType, Dictionary<string, dynamic> sessionAttribs)
        {


            TestLambdaContext context;
            TestLambdaLogger testLogger;
            AlexaRequest req;


            AlexaResponse returnVal;
            context = GetLambdaContext();
            testLogger = new TestLambdaLogger();
            context.Logger = testLogger;

            if (reqType == RequestType.IntentRequest)
                req = sessionContext.CreateIntentRequest(intent, slotValues, sessionAttribs);
            else
                req = sessionContext.CreateCanFulfillIntentRequest(intent, slotValues);

            returnVal = await function.FunctionHandlerAsync(req, context);

            //JsonSerializerSettings serSettings = new JsonSerializerSettings();
            //serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //serSettings.Formatting = Formatting.Indented;
            //string responseText = JsonConvert.SerializeObject(returnVal, serSettings);

            //string ssmlReturn = null;

            //if (!string.IsNullOrWhiteSpace(returnVal.Response?.OutputSpeech?.Ssml))
            //{
            //    ssmlReturn = returnVal.Response.OutputSpeech.Ssml;

            //}

            //testLogger = context.Logger as TestLambdaLogger;
            //testLog = testLogger.Buffer.ToString();
            //Debug.WriteLine(testLog);

            return returnVal;


        }




        protected async Task<AlexaResponse> GetIntentResult(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent)
        {
            return await GetIntentResult(sessionContext, function, intent, null, RequestType.IntentRequest);
        }

        protected async Task<AlexaResponse> GetIntentResult(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent, Dictionary<string, string> slots)
        {
            return await GetIntentResult(sessionContext, function, intent, slots, RequestType.IntentRequest);
        }


        protected async Task<AlexaResponse> GetIntentResult(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent, RequestType reqType)
        {
            return await GetIntentResult(sessionContext, function, intent, null, reqType);
        }


        protected async Task<AlexaResponse> GetLaunchResult(AlexaSessionContext sessionContext, AlexaFunctionBase function)
        {

            return await GetIntentResult(sessionContext, function, null, null, RequestType.LaunchRequest);
        }


        protected async Task<AlexaResponse> GetIntentResult(AlexaSessionContext sessionContext, AlexaFunctionBase function, string intent, Dictionary<string, string> slotValues, RequestType reqType)
        {


            TestLambdaContext context;
            TestLambdaLogger testLogger;
            AlexaRequest req = null;
            AlexaResponse returnVal;
            context = GetLambdaContext();
            testLogger = new TestLambdaLogger();
            context.Logger = testLogger;


            switch (reqType)
            {
                case RequestType.IntentRequest:
                    req = sessionContext.CreateIntentRequest(intent, slotValues, null);
                    break;
                case RequestType.CanFulfillIntentRequest:
                    req = sessionContext.CreateCanFulfillIntentRequest(intent, slotValues);
                    break;
                case RequestType.LaunchRequest:
                    req = sessionContext.CreateLaunchRequest();
                    break;
            }

            JsonSerializerSettings serSettings = new JsonSerializerSettings();
            //  serSettings.Formatting = Formatting.Indented;

            string requestType = JsonConvert.SerializeObject(req, serSettings);

            returnVal = await function.FunctionHandlerAsync(req, context);

            return returnVal;
        }






        public void WriteResponse(AlexaResponse resp)
        {
            if (resp == null)
            {
                Debug.WriteLine("Null response");
            }
            else
            {
                var cardResp = resp.Response?.Card;

                if (cardResp != null)
                {
                    Debug.WriteLine(string.Format("Card Response: {0}", cardResp.Title));
                    Debug.WriteLine("------");
                    Debug.WriteLine(cardResp.Content);
                    if (cardResp.ImageAttributes != null)
                    {
                        if (!string.IsNullOrWhiteSpace(cardResp.ImageAttributes.LargeImageUrl))
                            Debug.WriteLine(string.Format("Large Image Url: {0}", cardResp.ImageAttributes.LargeImageUrl));

                        if (!string.IsNullOrWhiteSpace(cardResp.ImageAttributes.SmallImageUrl))
                            Debug.WriteLine(string.Format("Small Image Url: {0}", cardResp.ImageAttributes.SmallImageUrl));

                    }
                }


                var outType = resp.Response?.OutputSpeech?.Type;

                if (outType.HasValue)
                {
                    if (outType.Value == OutputSpeechType.PlainText)
                    {

                        Debug.WriteLine("Plain Text Response");
                        Debug.WriteLine(" --------------");
                        Debug.WriteLine(resp.Response.OutputSpeech.Text);
                    }
                    else if (outType.Value == OutputSpeechType.Ssml)
                    {
                        Debug.WriteLine("SSML Response");
                        Debug.WriteLine(" --------------");
                        Debug.WriteLine(resp.Response.OutputSpeech.Ssml);
                    }
                }

                var reprompt = resp.Response?.Reprompt;

                if (reprompt != null)
                {
                    var repromptType = reprompt.OutputSpeech?.Type;

                    if (repromptType.HasValue)
                    {
                        if (repromptType.Value == OutputSpeechType.PlainText)
                        {

                            Debug.WriteLine("Plain Text Reprompt");
                            Debug.WriteLine(" --------------");
                            Debug.WriteLine(resp.Response.Reprompt.OutputSpeech.Text);
                        }
                        else if (repromptType.Value == OutputSpeechType.Ssml)
                        {
                            Debug.WriteLine("SSML Reprompt");
                            Debug.WriteLine(" --------------");
                            Debug.WriteLine(resp.Response.Reprompt.OutputSpeech.Ssml);
                        }


                    }

                }

                bool? shouldEndSession = resp.Response?.ShouldEndSession;


                if (shouldEndSession.HasValue)
                {
                    if (shouldEndSession.Value)
                        Debug.WriteLine("Session is ended");
                    else
                        Debug.WriteLine("Session remains open");

                }
                else
                    Debug.WriteLine("ShouldEndSession is missing");


                Debug.WriteLine("");
            }


        }
    }
}
