using Amazon.XRay.Recorder.Core;
using Amazon.XRay.Recorder.Core.Strategies;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Xunit;

namespace Whetstone.StoryEngine.Test.Google
{
    public class GoogleDialogFlowTest
    {



        [Fact]
        public async Task GetUserDynamoTestAsync()
        {
            List<Guid> userIds = new List<Guid>();
            for (int i = 0; i < 1000; i++)
            {
                userIds.Add(Guid.NewGuid());

            }

            var bag = new ConcurrentBag<TimingResponse>();
            var tasks = userIds.Select(async item =>
            {

                Stopwatch timing = Stopwatch.StartNew();
                TimingResponse timeResp = new TimingResponse();
                try
                {
                    await AnimalFarmPIEngineTest();
                }
                catch(Exception ex)
                {
                    timeResp.Ex = ex;

                }
                timing.Stop();
                timeResp.Milliseconds = timing.ElapsedMilliseconds;
                bag.Add(timeResp) ;
                
                Console.WriteLine($"User {item}");
            });
            await Task.WhenAll(tasks);
            var count = bag.Count;

            var overTimes = bag.Where(x => x.Milliseconds > 1000 || x.Ex!=null);
            foreach (var overTime in overTimes)
            {
                Console.WriteLine(overTime.Milliseconds);

                if(overTime.Ex!=null)
                    Console.WriteLine(overTime.Ex);
            }
            //EngineSessionContext sessionContext = new EngineSessionContext();

            //sessionContext.EngineUserId = user.Id.Value;
            //sessionContext.TitleVersion = titleVer;
            //sessionContext.EngineSessionId = Guid.NewGuid();

            // return sessionContext;

        }



        [Fact]
        public async Task AnimalFarmPIEngineTest()
        {
            AWSXRayRecorder.Instance.ContextMissingStrategy = ContextMissingStrategy.LOG_ERROR;
            try
            {

                  GoogleRequestGenerator requestGenerator = new GoogleRequestGenerator(new Uri("https://bnm0mhm104.execute-api.us-west-2.amazonaws.com/prod"), 
                    "drTc9thbDN3OLezGtJvnD35us9DGOiVd4dhJXNIT");
                //GoogleRequestGenerator requestGenerator = new GoogleRequestGenerator(new Uri("https://85ehi7exh8.execute-api.us-east-1.amazonaws.com/prod"),
                //  "EkyFBKJT5I9JiwDYuiKVf3FmldbHCTb12gljksh8");

                // https://z7ql7x8t57.execute-api.us-east-1.amazonaws.com/prod
               // string clientId = "amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0";

                //Client clientType = Models.Client.Alexa;

                string requestText = File.ReadAllText("Messages/dialogflowrequest01.json");


                JsonParser jsonParser = new JsonParser(JsonParser.Settings.Default.WithIgnoreUnknownFields(true));


                WebhookRequest request = jsonParser.Parse<WebhookRequest>(requestText);

                WebhookResponse resp = await requestGenerator.GetResponseAsync(request);

                string retPayload = resp.Payload.ToString();

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }

    //     StoryRequest req = engineContext.GetLaunchRequest();

    //    StoryResponse resp = await requestProcessor.ProcessStoryRequestAsync(req);


    //    await WriteResponseAsync(req, resp, clientType);


    //    req = engineContext.GetIntentRequest("StartInvestigationIntent", StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);

    //    //  await dataSessionLogger.LogRequestAsync(queueMessage);
    //    await WriteResponseAsync(req, resp, clientType);


    //    req = engineContext.GetIntentRequest("DarkAndStormyIntent", StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);


    //    await WriteResponseAsync(req, resp, clientType);


    //    req = engineContext.GetIntentRequest(ReservedIntents.YesIntent.Name, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    req = engineContext.GetIntentRequest(ReservedIntents.NoIntent.Name, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    Dictionary<string, string> location = new Dictionary<string, string> { { "FarmLocations", "tractor" } };
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "terrence");
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("character", "rat");
    //    req = engineContext.GetIntentRequest("TalkToIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "kitchen");
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    // End the session.
    //    req = engineContext.GetStopIntent();
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);



    //    // Relaunch the session.
    //    string userId = engineContext.IsRegisteredUser;

    //    engineContext = new EngineClientContext(titleVer, clientId, userId, clientType, "it-IT");

    //    req = engineContext.GetLaunchRequest();

    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);


    //    await WriteResponseAsync(req, resp, clientType);



    //    // I should be able to restart the story.
    //    req = engineContext.GetResumeRequest();
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);

    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>
    //{
    //    { "verb", "look" },
    //    { "item", "fridge" }
    //};
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);



    //    location = new Dictionary<string, string>
    //{
    //    { "verb", "try" },
    //    { "item", "cabbage" }
    //};
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>
    //{
    //    { "verb", "close" },
    //    { "item", "fridge" }
    //};
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("verb", "call");
    //    location.Add("character", "delivery service");
    //    req = engineContext.GetIntentRequest("VerbTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "outside");
    //    req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "barn");
    //    req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "milfred");
    //    req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "record");
    //    req = engineContext.GetIntentRequest("TurnOffItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("location", "garden");
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "catnip");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("location", "barn");
    //    req = engineContext.GetIntentRequest("GotoLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("item", "mackerel");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("item", "airhorn");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("verb", "play");
    //    location.Add("item", "airhorn");
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);



    //    location = new Dictionary<string, string>();
    //    location.Add("location", "barn");
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("location", "pond");
    //    req = engineContext.GetIntentRequest("GoToLocationIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "bread");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("verb", "look");
    //    location.Add("item", "wastebasket");
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("item", "bag");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "turnips");
    //    req = engineContext.GetIntentRequest("TakeItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "turnips");
    //    location.Add("verb", "throw");
    //    location.Add("character", "farmer");
    //    req = engineContext.GetIntentRequest("VerbTheItemAtTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "turnips");
    //    location.Add("verb", "throw");
    //    location.Add("character", "farmer");
    //    req = engineContext.GetIntentRequest("VerbTheItemAtTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("verb", "feed");
    //    location.Add("item", "ducks");
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    location = new Dictionary<string, string>();
    //    location.Add("verb", "open");
    //    location.Add("item", "briefcase");
    //    req = engineContext.GetIntentRequest("VerbTheItemIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);

    //    req = engineContext.GetIntentRequest("MakeSandwichIntent", StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "sandwich");
    //    location.Add("verb", "give");
    //    location.Add("character", "farmer");
    //    req = engineContext.GetIntentRequest("VerbTheItemToTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    location = new Dictionary<string, string>();
    //    location.Add("item", "sandwich");
    //    location.Add("verb", "give");
    //    location.Add("character", "terrence");
    //    req = engineContext.GetIntentRequest("VerbTheItemToTheCharacterIntent", location, StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


    //    req = engineContext.GetIntentRequest("EndGameIntent", StoryRequestType.Intent, resp.SessionContext);
    //    resp = await requestProcessor.ProcessStoryRequestAsync(req);
    //    await WriteResponseAsync(req, resp, clientType);


}

        private Task WriteResponseAsync(StoryRequest req, StoryResponse resp, Client clientType)
        {
            throw new NotImplementedException();
        }
    }
}
