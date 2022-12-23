using Amazon.Lambda.TestUtilities;
using Whetstone.Alexa;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Whetstone.StoryEngine;
using Amazon;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class ElderGodsTest : LambdaTestBase
    {
       
        internal const string userId = "amzn1.ask.account.AE3DZ3NJLES3Z5EM7IN3WO3ZH7LXFUGNZNVHHEDTD3YKRZMVPTKIQ6P3HR4WUHYI73XB4SG7E7AYQCXZUXOTIZRGZBLGL4R23HKINZTTWRH6VROUOFR4PLTLIT4HHJX34SNRPF3FNIMJSMFMRDC4N2GFRZNGRFGCWC2JHJMKDYHW5WORECJOHII7W4DDQOHNCJ3UXPTJKZQAOZQ";
        internal const string sessionId = "amzn1.echo-api.session.d3f663ca-8fff-47ce-b6a1-9c79d425bbef";

        [Fact(DisplayName = "Elder Gods Launch Tester")]
        public async Task LaunchElderGods()
        {
            var function = new AlexaFunctionProxy();


            AlexaSessionContext sessionContext = new AlexaSessionContext(ElderGodsSkilId, sessionId , "en-US", userId);
            

            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);

            resp = await GetIntentResult(sessionContext, function, "BeginIntent");
            

        }

        [Fact(DisplayName = "Elder Gods Walthrough")]
        public async Task ReplayElderGodsSession()
        {
            // ISessionRepository sessionRep = new DynamoDbSessionRepository(_envConfigOptions, _cache);
            ElderGodsTest parentTest = new ElderGodsTest();

            await parentTest.ElderGodsWalkthrough();
        }




        [Fact(DisplayName = "Elder Gods Walkthrough")]
        public async Task ElderGodsWalkthrough()
        {
            var function = new AlexaFunctionProxy();

            AlexaSessionContext sessionContext = new AlexaSessionContext(ElderGodsSkilId);

            sessionContext.UserId = "amzn1.ask.account.AG7OJCVYEBVLAF3AOIGWE44RSSXFPTOUKCU4UXZZ3F2QFTA6TJZBO2NIAZVZEXROBRE7IU4VXYRUR2JFPKSU52MEJGLUFHWOQ6BUCYYVPMQO2XZXS6MLXYFQFTJYJAHAGKKMUBYQADEJYCXOSAO4LG4H63IKERLH7H4HLJUEXS32UI6GGR2P7GGO3GP6V63DHJTEV57W4QQIAXQ";

            Debug.WriteLine(sessionContext.SessionId);


            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);

            resp = await GetIntentResult(sessionContext, function, "RestartIntent");
            WriteResponse(resp);

            Dictionary<string, string> verbItems = GetVerbTheItemSlots("ignore", "call");

            resp = await GetIntentResult(sessionContext, function, "VerbtheItemIntent", verbItems);
            WriteResponse(resp);

            Dictionary<string, string> location = GetLocationSlot("ceremony");

            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);

            verbItems = GetVerbTheActionSlots("read", "later");
            resp = await GetIntentResult(sessionContext, function, "VerbTheActionIntent", verbItems);
            WriteResponse(resp);

            string userId = sessionContext.UserId;
          

            location = GetLocationSlot("library");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            Dictionary<string, string> character = GetCharacterSlot("Margaret");
            resp = await GetIntentResult(sessionContext, function, "WaitForIntent", character);
            WriteResponse(resp);

            Dictionary<string, string> verbCharacter = GetVerbCharacterSlots("assist", "margaret");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbCharacter);
            WriteResponse(resp);

            verbItems = GetVerbTheItemSlots("look at", "picture");
            resp = await GetIntentResult(sessionContext, function, "VerbtheItemIntent", verbItems);
            WriteResponse(resp);


            verbItems = GetVerbCharacterSlots("visit", "artist");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbItems);
            WriteResponse(resp);

            // GO INSIDE
            location = GetLocationSlot("inside");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            // talk to Olivia

            // Search the house
            Dictionary<string, string> verbLocation = GetVerbTheLocationSlots("search", "house");
            resp = await GetIntentResult(sessionContext, function, "VerbtheLocationIntent", verbLocation);
            WriteResponse(resp);



            verbLocation = GetVerbTheLocationSlots("search", "room");
            resp = await GetIntentResult(sessionContext, function, "VerbtheLocationIntent", verbLocation);
            WriteResponse(resp);


            // Dictionary<string, string> talkToSlots = GetTalkToSlot("Olivia");
            //  resp = await GetIntentResult(sessionContext, function, "TalkToIntent", talkToSlots);
            // WriteResponse(resp);





            character = GetCharacterSlot("Lucas");
            resp = await GetIntentResult(sessionContext, function, "TalkToIntent", character);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);


            verbItems = GetVerbTheItemSlots("follow", "patient");
            resp = await GetIntentResult(sessionContext, function, "VerbTheItemIntent", verbItems);
            WriteResponse(resp);


            location = GetLocationSlot("home");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);


            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);

            verbCharacter = GetVerbCharacterSlots("call", "margaret");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbCharacter);
            WriteResponse(resp);

            verbItems = GetVerbTheActionSlots("insist", "helping");
            resp = await GetIntentResult(sessionContext, function, "VerbTheActionIntent", verbItems);
            WriteResponse(resp);


            verbCharacter = GetVerbCharacterSlots("ask", "margaret");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbCharacter);
            WriteResponse(resp);


            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);

            verbCharacter = GetVerbCharacterSlots("call", "Kelly");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbCharacter);
            WriteResponse(resp);

            verbItems = GetVerbTheItemSlots("stay", "phone");
            resp = await GetIntentResult(sessionContext, function, "VerbTheItemIntent", verbItems);
            WriteResponse(resp);

            verbItems = GetVerbTheItemSlots("search", "room");
            resp = await GetIntentResult(sessionContext, function, "VerbTheItemIntent", verbItems);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "GiveUpIntent");
            WriteResponse(resp);

            character = GetCharacterSlot("Kelly");
            resp = await GetIntentResult(sessionContext, function, "WaitForIntent", character);
            WriteResponse(resp);

           verbLocation = GetVerbTheLocationSlots("leave", "area");
            resp = await GetIntentResult(sessionContext, function, "VerbTheLocationIntent", verbLocation);
            WriteResponse(resp);

            location = GetLocationSlot("university");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            location = GetLocationSlot("McKnight Hall");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);


            verbItems = GetVerbTheItemSlots("finish", "ritual");
            resp = await GetIntentResult(sessionContext, function, "VerbTheItemIntent", verbItems);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "EndGameIntent");
            WriteResponse(resp);

            //ISessionReportingRepository sessionRep = new DynamoDbSessionReportingRepository(_envConfigOptions);

            //StorySession sessionRecord = await sessionRep.GetSessionAsync(sessionContext.SessionId);
            Debug.WriteLine(sessionContext.SessionId);

        }

        private Dictionary<string, string> GetTalkToSlot(string character)
        {
            Dictionary<string, string> talkToSlot = new Dictionary<string, string>();

            talkToSlot.Add("character", character);

            return talkToSlot;
        }

        [Fact(DisplayName = "Elder Gods Walthrough")]
        public async Task ElderGodsArtistVisitWalkthrough()
        {
            var function = new AlexaFunctionProxy();
            AlexaSessionContext sessionContext = new AlexaSessionContext(ElderGodsSkilId);

            var context = GetLambdaContext();

            AlexaResponse resp = await GetLaunchResult(sessionContext, function);

            resp = await GetIntentResult(sessionContext, function, "StartInvestigationIntent");
            WriteResponse(resp);


            Dictionary<string, string> verbItems = GetVerbTheItemSlots("answer", "phone");

            resp = await GetIntentResult(sessionContext, function, "VerbtheItemIntent", verbItems);
            WriteResponse(resp);

            Dictionary<string, string> location = GetLocationSlot("ceremony");

            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            resp = await GetIntentResult(sessionContext, function, "GoOnIntent");
            WriteResponse(resp);

            verbItems = GetVerbTheActionSlots("read", "later");
            resp = await GetIntentResult(sessionContext, function, "VerbTheActionIntent", verbItems);
            WriteResponse(resp);

            string userId = sessionContext.UserId;


            location = GetLocationSlot("library");
            resp = await GetIntentResult(sessionContext, function, "GotoLocationIntent", location);
            WriteResponse(resp);

            Dictionary<string, string> character = GetCharacterSlot("Margaret");
            resp = await GetIntentResult(sessionContext, function, "WaitForIntent", character);
            WriteResponse(resp);

            Dictionary<string, string> verbCharacter = GetVerbCharacterSlots("assist", "margaret");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbCharacter);
            WriteResponse(resp);

            verbItems = GetVerbTheItemSlots("look at", "picture");
            resp = await GetIntentResult(sessionContext, function, "VerbtheItemIntent", verbItems);
            WriteResponse(resp);

            verbItems = GetVerbCharacterSlots("visit", "artist");
            resp = await GetIntentResult(sessionContext, function, "VerbTheCharacterIntent", verbItems);
            WriteResponse(resp);




        }



        private Dictionary<string, string> GetVerbTheLocationSlots(string verb, string location)
        {
            Dictionary<string, string> verbLocation = new Dictionary<string, string>();

            verbLocation.Add("verb", verb);
            verbLocation.Add("location", location);

            return verbLocation;
        }

        private Dictionary<string, string> GetVerbTheActionSlots(string verb, string action)
        {
            Dictionary<string, string> verbAction = new Dictionary<string, string>();

            verbAction.Add("verb", verb);
            verbAction.Add("action",action);

            return verbAction;
        }

        private new void WriteResponse(AlexaResponse resp)
        {
           if(resp==null)
            {
                Debug.WriteLine("Null response");
            }
           else
            {
                var cardResp = resp.Response?.Card;

                if(cardResp!=null)
                {
                    Debug.WriteLine(string.Format("Card Response: {0}", cardResp.Title));
                    Debug.WriteLine("------");
                    Debug.WriteLine(cardResp.Content);
                    if(cardResp.ImageAttributes!=null)
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

                if(reprompt!=null)
                {
                    var repromptType = reprompt.OutputSpeech?.Type;

                    if(repromptType.HasValue)
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

        private Dictionary<string, string> GetCharacterSlot(string character)
        {
            Dictionary<string, string> verbItems = new Dictionary<string, string>();

            verbItems.Add("character", character);
         
            return verbItems;

        }

        private Dictionary<string, string> GetVerbCharacterSlots(string verb, string character)
        {
            Dictionary<string, string> verbChar = new Dictionary<string, string>();

            verbChar.Add("verb", verb);
            verbChar.Add("character", character);

            return verbChar;
        }

        private Dictionary<string, string> GetVerbTheItemSlots(string verb, string item)
        {
            Dictionary<string, string> verbItems = new Dictionary<string, string>();

            verbItems.Add("verb", verb);
            verbItems.Add("item", item);

            return verbItems;

        }

        private Dictionary<string, string> GetLocationSlot(string location)
        {

            Dictionary<string, string> locSlots = new Dictionary<string, string>();

            locSlots.Add("location", location);

            return locSlots;
        }


    }
}
