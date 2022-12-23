
using Amazon.Lambda.TestUtilities;
using Newtonsoft.Json;
using Whetstone.Alexa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Serialization;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.FileStorage;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Configuration;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Whetstone.StoryEngine.Models.Tracking;
using Whetstone.StoryEngine.Models.Validation;
using Xunit;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class AnimalFarmTester : LambdaTestBase
    {

        [Fact(DisplayName = "Launch Barn Test")]
        public async Task GoToBarnTest()
        {
            TestLambdaLogger testLogger;
            string testLog;

            string locale = "en-US";

            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.AnimalFarmPIId, string.Concat("testsession_", Guid.NewGuid().ToString()),locale);

            var context = GetLambdaContext();

            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var function = new AlexaFunctionProxy();
            var returnVal = await function.FunctionHandlerAsync(req, context);

            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            if (testLogger != null)
            {
                testLog = testLogger.Buffer.ToString();
                Debug.WriteLine(testLog);
            }

            string retValText = JsonConvert.SerializeObject(returnVal);
            AlexaResponse alexaRet = JsonConvert.DeserializeObject<AlexaResponse>(retValText);
            Dictionary<string, dynamic> sessAttribs = alexaRet.SessionAttributes;


            AlexaResponse intentResp = await SendIntent(sessionContext, function, "StartInvestigationIntent", sessAttribs);

            Dictionary<string, string> location = new Dictionary<string, string>();

            intentResp = await SendIntent(sessionContext, function, "DarkAndStormyIntent", intentResp.SessionAttributes);

            intentResp = await SendIntent(sessionContext, function, "AMAZON.NoIntent", intentResp.SessionAttributes);

            intentResp = await SendIntent(sessionContext, function, "AMAZON.YesIntent", intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            //   location.Add("location", "kitchen");
            //   location.Add("location", "barn");
            //  location.Add("location", "tractor");

            location.Add("FarmLocations", "tractor");

            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("location", "terrence");
            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("character", "rat");
            intentResp = await SendIntent(sessionContext, function, "TalkToIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("location", "kitchen");
            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "look");
            location.Add("item", "fridge");
            intentResp = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("verb", "try");
            location.Add("item", "cabbage");
            intentResp = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "close");
            location.Add("item", "fridge");
            intentResp = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "call");
            location.Add("character", "delivery service");
            intentResp = await SendIntent(sessionContext, function, "VerbTheCharacterIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "outside");
            intentResp = await SendIntent(sessionContext, function, "GotoLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

           
            location = new Dictionary<string, string>();
            location.Add("location", "barn");
            intentResp = await SendIntent(sessionContext, function, "GotoLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "milfred");
            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);
            

            location = new Dictionary<string, string>();
            location.Add("item", "record");
            intentResp = await SendIntent(sessionContext, function, "TurnOffItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("location", "garden");
            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);



            location = new Dictionary<string, string>();      
            location.Add("item", "catnip");
            intentResp = await SendIntent(sessionContext, function, "TakeItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("location", "barn");
            intentResp = await SendIntent(sessionContext, function, "GotoLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("item", "mackerel");
            intentResp = await SendIntent(sessionContext, function, "TakeItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("item", "airhorn");
            intentResp = await SendIntent(sessionContext, function, "TakeItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("verb", "play");
            location.Add("item", "airhorn");
            intentResp = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "barn");
            intentResp = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, intentResp.SessionAttributes);


        }

        [Fact(DisplayName = "Launch Animal Farm Test")]
        public async Task LaunchAnimalFarm()
        {


            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();

            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.AnimalFarmPIId, Guid.NewGuid().ToString(), "en-UK");
            //AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.AnimalFarmTestId, Guid.NewGuid().ToString(), "en-US");


            var context = GetLambdaContext();
            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var returnVal = await function.FunctionHandlerAsync(req, context);


            //JsonSerializerSettings serSettings = new JsonSerializerSettings();
            //serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //serSettings.Formatting = Formatting.Indented;
       

            //string responseText = JsonConvert.SerializeObject(returnVal, serSettings);

            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            testLog = testLogger.Buffer.ToString();
    
           Debug.WriteLine(testLog);


           AlexaResponse ssmlText = await SendIntent(sessionContext, function, "StartInvestigationIntent", returnVal.SessionAttributes);


            Dictionary<string,string> location  = new Dictionary<string, string>();

                         
            ssmlText = await SendIntent(sessionContext, function, "DarkAndStormyIntent", returnVal.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "EndGameIntent", returnVal.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "AMAZON.NoIntent", returnVal.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "AMAZON.YesIntent", returnVal.SessionAttributes);

  


            location = new Dictionary<string, string>();
         //   location.Add("location", "kitchen");
         //   location.Add("location", "barn");
            location.Add("location", "tractor");

            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("location", "terrence");
            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("character", "rat");
            ssmlText = await SendIntent(sessionContext, function, "TalkToIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("location", "kitchen");
            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "look");
            location.Add("item", "fridge");
            ssmlText = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("verb", "try");
            location.Add("item", "cabbage");
            ssmlText = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "close");
            location.Add("item", "fridge");
            ssmlText = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("verb", "call");
            location.Add("character", "delivery service");
            ssmlText = await SendIntent(sessionContext, function, "VerbTheCharacterIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "outside");
            ssmlText = await SendIntent(sessionContext, function, "GotoLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "barn");
            ssmlText = await SendIntent(sessionContext, function, "GotoLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "milfred");
            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("item", "record");
            ssmlText = await SendIntent(sessionContext, function, "TurnOffItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("item", "mackerel");
            ssmlText = await SendIntent(sessionContext, function, "TakeItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("item", "airhorn");
            ssmlText = await SendIntent(sessionContext, function, "TakeItemIntent", location,RequestType.IntentRequest, returnVal.SessionAttributes);

            location = new Dictionary<string, string>();
            location.Add("verb", "play");
            location.Add("item", "airhorn");
            ssmlText = await SendIntent(sessionContext, function, "VerbTheItemIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);


            location = new Dictionary<string, string>();
            location.Add("location", "barn");
            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);



            location = new Dictionary<string, string>();
            location.Add("location", "kitchen");

            // Going to the kitch should produce an error at this point.
            ssmlText = await SendIntent(sessionContext, function, "GoToLocationIntent", location, RequestType.IntentRequest, returnVal.SessionAttributes);




            location = new Dictionary<string, string>();
            location.Add("item", "mackerel");


            ssmlText = await SendIntent(sessionContext, function, "TakeItemIntent", ssmlText.SessionAttributes);




            ssmlText = await SendIntent(sessionContext, function, "WakeMilfredIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "TakeMackerelIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "PickupAirhornIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "PickCatnipIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "PlayAirhornIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "BarnIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "KitchenIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "PhoneCallIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "RefrigeratorIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "CloseTheFridgeIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "CallDeliveryServiceIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "GoOutsideIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "PondIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "LookInWastebasketIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "TakeTheBagIntent", ssmlText.SessionAttributes);



            ssmlText = await SendIntent(sessionContext, function, "TakeTheTurnipsIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "ThrowTurnipsIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "ThrowTurnipsIntent", ssmlText.SessionAttributes);


            ssmlText = await SendIntent(sessionContext, function, "FeedTheDucksIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "OpenTheBriefcaseIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "MakeSandwichIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "GiveRatSandwichIntent", ssmlText.SessionAttributes);

     


        }

        [Fact(DisplayName = "Resume animal farm")]
        public async Task ResumeAnimalFarm()
        {

            // AMAZON.ResumeIntent



            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();

            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.AnimalFarmPIId, "amzn1.echo-api.session.905971e8-fecb-4ccf-9350-36f4199b4779");


            var context = GetLambdaContext();
            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var returnVal = await function.FunctionHandlerAsync(req, context);
            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;
            testLog = testLogger.Buffer.ToString();


            Debug.WriteLine(testLog);
            AlexaResponse ssmlText = await SendIntent(sessionContext, function, "AMAZON.ResumeIntent", returnVal.SessionAttributes);
        }


        [Fact(DisplayName = "Launch Animal Bad Intent")]
        public async Task LaunchAnimalFarmBadIntent()
        {
            TestLambdaLogger testLogger;
            string testLog;
            var function = new AlexaFunctionProxy();

            AlexaSessionContext sessionContext = new AlexaSessionContext(AlexaSessionContext.AnimalFarmPIId);


            var context = GetLambdaContext();
            AlexaRequest req = sessionContext.CreateLaunchRequest();

            var returnVal = await function.FunctionHandlerAsync(req, context);
            Debug.WriteLine(returnVal.Response.OutputSpeech.Ssml);
            testLogger = context.Logger as TestLambdaLogger;

            if (testLogger == null)
            {
                var debugLogger = context.Logger as DebugLogger;

            }
            else
            {
                testLog = testLogger.Buffer.ToString();
                Debug.WriteLine(testLog);
            }

            AlexaResponse ssmlText = await SendIntent(sessionContext, function, "BeginIntent", returnVal.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "DarkPathIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "CallDeliveryServiceIntent", ssmlText.SessionAttributes);

            ssmlText = await SendIntent(sessionContext, function, "AMAZON.RepeatIntent", ssmlText.SessionAttributes);
        }



 

        private AlexaRequest GetRequestByFile(string fileName)
        {

            string jsonRawText = File.ReadAllText(fileName);

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(jsonRawText);
            return req;
        }






        private void AddInventoryAction(StoryTitle adventureTitle, string nodeName, UniqueItem sauerkrautItem)
        {
            var actionNode = adventureTitle.Nodes.FirstOrDefault(x => x.Name.Equals(nodeName, StringComparison.OrdinalIgnoreCase));
            if (actionNode != null)
            {
                actionNode.Actions = new List<NodeActionData>();

                InventoryActionData addItem = new InventoryActionData();
                addItem.ActionType = InventoryActionType.Add;
                addItem.Item = sauerkrautItem;

                actionNode.Actions = new List<NodeActionData>();
                actionNode.Actions.Add(addItem);
            }



        }


        [Fact(DisplayName = "Update Animal farm PI")]
        public async Task UpdateAnimalFarmFromS3()
        {

            TitleVersion titleVer = new TitleVersion("animalfarmpi", "1.0");

            StoryTitle adventureTitle = await this.GetTitleAsync(titleVer);

            foreach (var node in adventureTitle.Nodes)
            {
                foreach(var respSet in node.ResponseSet)
                {

                    foreach(var locResp in respSet.LocalizedResponses)
                    {
                        if(string.IsNullOrWhiteSpace(locResp.LargeImageFile))                        
                            locResp.LargeImageFile = "afi-welcome-lg.png";
                        
                        if(string.IsNullOrWhiteSpace(locResp.SmallImageFile))
                            locResp.SmallImageFile = "afi-welcome-sm.png";


                    }

                }

            }



            var ser = YamlSerializationBuilder.GetYamlSerializer();

            string rawText = ser.Serialize(adventureTitle);
            

        }

  
    }
}
