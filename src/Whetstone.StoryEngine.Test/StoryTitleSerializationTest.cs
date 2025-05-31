using MessagePack;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whetstone.Alexa;
using Whetstone.Alexa.Security;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Messaging.Sms;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Xunit;
using YamlDotNet.Core;

namespace Whetstone.StoryEngine.Test
{
    public class StoryTitleSerializationTest : TestServerFixture
    {

        [Fact]
        public void EditWhetstoneSonibrigeCardResponses()
        {
            string text = File.ReadAllText("importfiles/whetstonetechnologies/0.3/whetstonetechnologies.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            var sonibridgeNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals("SoniBridgeInfo"));

            var cardResponse = sonibridgeNode.ResponseSet[0].LocalizedResponses[0].CardResponses;

            cardResponse[1].SpeechClient = Client.Bixby;



            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string whetstoneText = yamlSer.Serialize(retTitle);
        }


        [Fact]
        public void AddLinkButton()
        {
            string text = File.ReadAllText("importfiles/whetstonetechnologies/0.4/whetstonetechnologies.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            StoryNode linkNode = new StoryNode
            {
                Name = "ReturnLink",

                ResponseSet = new List<LocalizedResponseSet>()
            };
            LocalizedResponseSet respSet = new LocalizedResponseSet
            {
                LocalizedResponses = new List<LocalizedResponse>()
            };

            LocalizedResponse locResp = new LocalizedResponse
            {
                TextFragments = new List<TextFragmentBase>()
            };

            SimpleTextFragment textFrag = new SimpleTextFragment("Thanks for your interest in Whetstone Technologies! Please go to the following link to get your FREE whitepaper.");

            locResp.TextFragments.Add(textFrag);


            locResp.CardResponses = new List<CardResponse>();

            CardResponse cardResp = new CardResponse
            {
                Buttons = new List<CardButton>()
            };

            LinkButton linkButton = new LinkButton
            {
                LinkText = "Whitepaper",
                Url = "https://bit.ly/2XQrJXh"
            };
            cardResp.Buttons.Add(linkButton);


            locResp.CardResponses.Add(cardResp);

            respSet.LocalizedResponses.Add(locResp);
            linkNode.ResponseSet.Add(respSet);

            retTitle.Nodes.Add(linkNode);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            string whestoneTechNew = yamlSer.Serialize(retTitle);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }


        [Fact]
        public void AddChoiceOption()
        {
            string text = File.ReadAllText("importfiles/whetstonetechnologies/0.3/whetstonetechnologies.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            var verificationNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals("GetPhoneNumberVerification"));

            var yesChoice = verificationNode.Choices.FirstOrDefault(x => x.IntentName.Equals("YesIntent"));



            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string animalFarmPi = yamlSer.Serialize(retTitle);
        }



        [Fact]
        public void EditWhetstoneTechSms()
        {
            string text = File.ReadAllText("importfiles/whetstonetechnologiessms/whetstonetechnologiessms.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            retTitle.PhoneInfo.RequiredConsents = new List<string>
            {
                "whetstonetechnologiese"
            };

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

#pragma warning disable IDE0059 // Unnecessary assignment of a value
            string animalFarmPi = yamlSer.Serialize(retTitle);
#pragma warning restore IDE0059 // Unnecessary assignment of a value
        }



        [Fact]
        public void DeserializeAnimalFarmProd()
        {


            string text = File.ReadAllText("importfiles/animalfarmpi/prod/animalfarmpi.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string animalFarmPi = yamlSer.Serialize(retTitle);


        }

        [Fact]
        public void AddErrorNode()
        {


            string text = File.ReadAllText("importfiles/animalfarmpi/animalfarmpi.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);







            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string animalFarmPi = yamlSer.Serialize(retTitle);


        }

        [Fact]
        public async Task SerializeWhetstoneAsyncTest()
        {
            IAppMappingReader appReader = Services.GetRequiredService<IAppMappingReader>();
            string clientId = "amzn1.ask.skill.c4cabd50-2cd5-4e4c-a03c-a57d4f2a0e5f";

            Client clientType = Models.Client.Alexa;
            TitleVersion titleVer = await appReader.GetTitleAsync(clientType, clientId, null);


            ITitleReader titleReader = Services.GetRequiredService<ITitleReader>();

            StoryTitle whetstoneStory = await titleReader.GetByIdAsync(titleVer);


            Intent learnWhet = whetstoneStory.Intents.FirstOrDefault(x => x.Name.Equals("LearnAboutWhetstone", StringComparison.OrdinalIgnoreCase));

            learnWhet.SupportsNamelessInvocation = true;

            //            InvalidNumberNode
            //CannotGetSmsNumberNode

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string animalFarmPi = yamlSer.Serialize(whetstoneStory);


        }


        [Fact]
        public void LoadInsuranceAgent()
        {
            StoryTitle retTitle = GetFileTitle("insuranceagent");

            var newUserNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals("WelcomeNewUser"));


            var newDriverChoice = newUserNode.Choices.FirstOrDefault(x => x.IntentName.Equals("AddNewDriverIntent"));

            ConditionalNodeMapping condMapping = new ConditionalNodeMapping
            {
                Conditions = new List<string>(),

                TrueConditionResult = new SingleNodeMapping { NodeName = "AddNewDriver" },

                FalseConditionResult = new SingleNodeMapping { NodeName = "GetFirstName" }
            };


            newDriverChoice.NodeMapping = condMapping;

            if (retTitle.Conditions == null)
            {
                retTitle.Conditions = new List<StoryConditionBase>();

            }

            //Choice curChoice = firstName.Choices[0];

            //curChoice.Actions = new List<NodeActionData>();

            //curChoice.Actions.Add(recItem);

            //var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            //string titleText = yamlSer.Serialize(retTitle);


            // var choice =  addDriverNode.Choices[0];



        }

        [Fact]
        public void EyeOfTheElderGodsChoiceSelection()
        {
            string text = File.ReadAllText("importfiles/eyeoftheeldergods/eyeoftheeldergods.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            var goHomeNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals("C6"));

            List<String> suggestions = goHomeNode.Choices.GetSuggestions("en-US", null, null);


        }

        [Fact]
        public void LoadAnimalFarmJson()
        {
            string text = File.ReadAllText("importfiles/animalfarmpi/animalfarmpi.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);




            UserClientCondition userCond = new UserClientCondition
            {
                ClientType = Models.Client.Alexa,

                Name = "AlexaClient"
            };

            retTitle.Conditions.Add(userCond);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string animalFarmPi = yamlSer.Serialize(retTitle);


        }

        [Fact]
        [Trait("Category", "Macro")]
        public void GetMacroValues()
        {
            string regExpression = @"(?<=\{)[^}]*(?=\})";

            Regex regex = new Regex(regExpression, RegexOptions.IgnoreCase);

            MatchCollection matches = regex.Matches("I heard you say <say-as interpret-as=\"telephone\">{phonenumber}</say-as>. Is that correct?");
            // Results include braces (undesirable)
            var results = matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }



        [Trait("Category", "Serialization")]
        [Fact(DisplayName = "Story Request Round Trip")]
        public void StoryRequestRoundTrip()
        {

            StoryRequest req = new StoryRequest();
            string deviceId = "34230894723084";

            req.Client = Models.Client.Alexa;
            AlexaSecurityInfo alexaInfo = new AlexaSecurityInfo
            {
                AccessToken = "123",
                DeviceId = deviceId
            };

            req.SecurityInfo = new Dictionary<string, string>
            {
                { "deviceId", deviceId }
            };

            req.RequestType = StoryRequestType.Intent;
            req.Locale = "en-US";

            req.UserId = "234523423";

            string jsonText = JsonConvert.SerializeObject(req);


            StoryRequest returnedReq = JsonConvert.DeserializeObject<StoryRequest>(jsonText);

            Assert.NotNull(returnedReq.SecurityInfo);

            var returnedSecInfo = returnedReq.SecurityInfo;

            Assert.Equal(deviceId, returnedSecInfo["deviceId"]);
        }



        [Fact]
        public void LoadStoryJson()
        {
            string text = File.ReadAllText("importfiles/jsonfile/animalfarmtest.json");

            JsonSerializerSettings serSettings = GetJsonSettings();

            StoryTitle retTitle = null;

            retTitle = JsonConvert.DeserializeObject<StoryTitle>(text, serSettings);

        }

        [Fact]
        public void StatileanIntentTest()
        {
            string text = File.ReadAllText("importfiles/statileansavings/statileansavings.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle testTitle = null;
            testTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            StoryNode getPhoneNode = testTitle.Nodes.FirstOrDefault(x => x.Name.Equals("StatileanRegularDiscount"));



            //getPhoneNode.Choices = new List<Choice>();

            Choice phoneChoice = new Choice
            {
                IntentName = "PhoneNumberIntent"
            };

            ConditionalNodeMapping condMapping = new ConditionalNodeMapping
            {
                Conditions = new List<string>()
            };
            condMapping.Conditions.Add("supportssmscond");

            condMapping.TrueConditionResult = new SingleNodeMapping("PhoneDiscountVerification");


            // Structure format error result

            ConditionalNodeMapping notsupportSmsMapping = new ConditionalNodeMapping
            {
                Conditions = new List<string>()
            };
            notsupportSmsMapping.Conditions.Add("isphonenumbervalidcond");
            notsupportSmsMapping.TrueConditionResult = new SingleNodeMapping("CannotGetSmsMessageNode");
            notsupportSmsMapping.FalseConditionResult = new SingleNodeMapping("BadPhoneFormatNode");
            condMapping.FalseConditionResult = notsupportSmsMapping;

            phoneChoice.NodeMapping = condMapping;

            getPhoneNode.Choices = new List<Choice>
            {
                phoneChoice
            };

            var yamlDeser = YamlSerializationBuilder.GetYamlSerializer();

            string configText = yamlDeser.Serialize(testTitle);

        }




        [Fact]
        public void SsmlBreakSerializationTest()
        {
            string text = File.ReadAllText("importfiles/SsmlBreakTest/SsmlBreakTest.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle testTitle = null;
            StoryTitle packedTitle = null;

            try
            {
                testTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

                var speechResponse = testTitle.Nodes[0].ResponseSet[0].LocalizedResponses[0].SpeechResponses[0];

                byte[] byteVal = null;
                using (MemoryStream memStream = new MemoryStream())
                {
                    MessagePackSerializer.Serialize<StoryTitle>(memStream, testTitle, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);
                    byteVal = memStream.ToArray();
                }

                using (MemoryStream memStream = new MemoryStream(byteVal))
                {
                    packedTitle = MessagePack.MessagePackSerializer.Deserialize<StoryTitle>(memStream, MessagePack.Resolvers.TypelessContractlessStandardResolver.Options);
                }

                var packedResponse = packedTitle.Nodes[0].ResponseSet[0].LocalizedResponses[0].SpeechResponses[0];

            }
            catch (YamlException yex)
            {
                Debug.WriteLine(yex);
                throw;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;

            }

        }

        [Fact(DisplayName = "Load Discount Coupon Finder")]
        public void LoadDiscountCouponFinder()
        {


            string text = File.ReadAllText("importfiles/discountcouponstest/discountcouponstest.yaml");
            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle trialsTitle = null;

            try
            {
                trialsTitle = yamlDeserializer.Deserialize<StoryTitle>(text);
            }
            catch (YamlException yex)
            {
                Debug.WriteLine(yex);
                throw;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;

            }


            // add a node action to the phone number verification

            StoryNode foundNode = trialsTitle.Nodes.FirstOrDefault(x => x.Name.Equals("SendDiscountCodeNode"));

            if (foundNode != null)
            {
                PhoneMessageActionData messageAction = new PhoneMessageActionData
                {
                    PhoneNumberSlot = "phonenumber",
                    Messages = new List<PhoneMessage>()
                };

                StringBuilder textMessage = new StringBuilder();
                textMessage.AppendLine("See terms at http://goo.gl/SlplnU");
                textMessage.AppendLine();
                textMessage.AppendLine("Reply YES to agree to program/ offer terms & complete your enrollment. Msg & Data rates may apply.Text HELP for info, STOP to end.");

                PhoneMessage phoneMessage = new PhoneMessage
                {
                    Message = textMessage.ToString()
                };
                messageAction.Messages.Add(phoneMessage);

                foundNode.Actions = new List<NodeActionData>
                {
                    messageAction
                };
            }

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(trialsTitle);

        }

        [Fact(DisplayName = "Load Clinical Trial")]
        public void LoadClinicalTrial()
        {


            string text = File.ReadAllText("importfiles/clinicaltrialstest/clinicaltrialstest.yaml");
            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle trialsTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            trialsTitle.StoryType = StoryType.SingleRequest;

            var foundNode = trialsTitle.Nodes.FirstOrDefault(x => x.Name.Equals("WelcomeNewUser"));


            var locResp = foundNode.ResponseSet[0].LocalizedResponses[0];

            SimpleTextFragment multiLineFragment = new SimpleTextFragment();

            StringBuilder multiLine = new StringBuilder();

            multiLine.AppendLine("This is the first line.");
            multiLine.AppendLine("This is the second line.");
            multiLine.AppendLine("This is the third line.");

            multiLineFragment.Text = multiLine.ToString();

            locResp.TextFragments.Add(multiLineFragment);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(trialsTitle);
        }


        [Fact(DisplayName = "Load Test Title")]
        public void LoadTestTitle()
        {


            string text = File.ReadAllText("importfiles/animalfarmtest/animalfarmtest.yaml");
            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle globalConfig = yamlDeserializer.Deserialize<StoryTitle>(text);



            var node = globalConfig.Nodes.FirstOrDefault();

            node.Coordinates = new Coordinates(10, 15);

            JsonSerializerSettings ser = GetJsonSettings();

            ser.Formatting = Formatting.Indented;
            string jsonText = JsonConvert.SerializeObject(globalConfig, ser);


            StoryTitle jsonTitle = null;


            try
            {

                jsonTitle = JsonConvert.DeserializeObject<StoryTitle>(jsonText, ser);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();


            string yamlOut = yamlSer.Serialize(jsonTitle);

            // And back from Yaml

            StoryTitle postYaml = yamlDeserializer.Deserialize<StoryTitle>(yamlOut);

            //IFileRepository fileRep = new S3FileStore(_envConfigOptions, _cache);

            //ITitleReader titleReader = new YamlTitleReader(fileRep);

            //var adventureTitle = titleReader.GetByIdAsync("test_adventure").Result;

        }


        //[Fact(DisplayName = "Load Story Map")]
        //public void LoadStoryLogMap()
        //{
        //    string text = File.ReadAllText("samplemessages/samplemessage.json");

        //    try
        //    {
        //        SessionLogQueueMessage sessionLog = JsonConvert.DeserializeObject<SessionLogQueueMessage>(text);
        //    }
        //    catch(Exception ex)
        //    {

        //        throw;
        //    }

        //}


        [Fact(DisplayName = "Load ElderGods Title")]
        public void LoadElderGodsTitle()
        {


            string text = File.ReadAllText("importfiles/eyeoftheeldergods/eyeoftheeldergods.yaml");
            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle globalConfig = yamlDeserializer.Deserialize<StoryTitle>(text);


            var firstNode = globalConfig.Nodes.FirstOrDefault(x => x.Name.Equals("A1"));

            firstNode.Actions = new List<NodeActionData>();

            NodeVisitRecordActionData visitAction = new NodeVisitRecordActionData
            {
                IsPermanent = true
            };

            firstNode.Actions.Add(visitAction);

            var saveWorldNode = globalConfig.Nodes.FirstOrDefault(x => x.Name.Equals("D4"));
            saveWorldNode.Actions = new List<NodeActionData>
            {
                visitAction
            };


            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();


            // And back from Yaml

            string postYaml = yamlSer.Serialize(globalConfig);

            //IFileRepository fileRep = new S3FileStore(_envConfigOptions, _cache);

            //ITitleReader titleReader = new YamlTitleReader(fileRep);

            //var adventureTitle = titleReader.GetByIdAsync("test_adventure").Result;

        }


        [Fact(DisplayName = "Get Story Title Yaml")]
        public void GetStoryNode()
        {
            IFileReader fileRep = Services.GetService<S3FileReader>();

            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            var distCache = GetMemoryCache();

            ILogger<TitleCacheRepository> titleLogger = Services.GetService<ILogger<TitleCacheRepository>>();



            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, distCache, memCache, titleLogger);

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVersion = new TitleVersion("test_adventure", "1.0");

            var adventureTitle = titleReader.GetByIdAsync(titleVersion).Result;

        }


        [Fact(DisplayName = "Get Go Back Intents Yaml")]
        public void GetGoBackIntents()
        {
            IFileRepository fileRep = Services.GetService<S3FileStore>();

            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            var memDict = GetMemoryCache();

            ILogger<TitleCacheRepository> titleLogger = Services.GetService<ILogger<TitleCacheRepository>>();


            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, memDict, memCache, titleLogger);

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);
            TitleVersion titleVersion = new TitleVersion("eyeoftheeldergods", "1.0");

            var adventureTitle = titleReader.GetByIdAsync(titleVersion).Result;

            foreach (StoryNode node in adventureTitle.Nodes)
            {
                if ((node.Choices?.Any()).GetValueOrDefault(false))
                {
                    foreach (Choice choice in node.Choices)
                    {
                        if (choice.IntentName.EndsWith("GoBackIntent", StringComparison.OrdinalIgnoreCase))
                        {
                            if (choice.NodeMapping is SingleNodeMapping singleNode)
                            {
                                if ((choice.ConditionNames?.Any()).GetValueOrDefault(false))
                                {
                                    string conditions = choice.ConditionNames.JoinList();

                                    Debug.WriteLine(string.Format("{0} --> {1} on conditions(s) {2}", node.Name, singleNode.NodeName, conditions));
                                }
                                else
                                    Debug.WriteLine(string.Format("{0} --> {1}", node.Name, singleNode.NodeName));

                            }
                            else
                                Debug.WriteLine("Unexpected mapping");
                        }


                    }


                }



            }


        }


        [Fact(DisplayName = "Get Go Back Intents Yaml")]
        public void ListTextResponses()
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();
            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("eyeoftheeldergods", "1.0");

            var adventureTitle = titleReader.GetByIdAsync(titleVer).Result;
            StringBuilder sb = new StringBuilder();

            foreach (StoryNode node in adventureTitle.Nodes)
            {
                sb.AppendFormat("Node: {0}", node.Name);
                sb.AppendLine();
                foreach (LocalizedResponseSet respSet in node.ResponseSet)
                {

                    foreach (LocalizedResponse locResp in respSet.LocalizedResponses)
                    {


                        sb.AppendFormat("\tCard Title: {0}", locResp.CardTitle);
                        sb.AppendLine();
                        sb.AppendFormat("\tText Response:", locResp.CardTitle);
                        sb.AppendLine();

                        if ((locResp.TextFragments?.Any()).GetValueOrDefault(false))
                        {
                            foreach (TextFragmentBase textBase in locResp.TextFragments)
                            {
                                //                            [MessagePack.Union(0, typeof(SimpleTextFragment))]
                                //[MessagePack.Union(1, typeof(ConditionalTextFragment))]

                                if (textBase is SimpleTextFragment simpleFrag)
                                {
                                    sb.AppendFormat("\t\t{0}", simpleFrag.Text);
                                    sb.AppendLine();

                                }


                            }


                        }

                        sb.AppendLine();




                        sb.AppendLine("\tReprompt:");

                        if (string.IsNullOrWhiteSpace(locResp.RepromptTextResponse))
                        {
                            if ((locResp.RepromptSpeechResponses?.Any()).GetValueOrDefault(false))
                            {
                                foreach (ClientSpeechFragments speechResp in locResp.RepromptSpeechResponses)
                                {
                                    foreach (SpeechFragment frag in speechResp.SpeechFragments)
                                    {
                                        if (frag is PlainTextSpeechFragment plainFrag)
                                        {
                                            sb.AppendFormat("\t\t{0}", plainFrag.Text);
                                            sb.AppendLine();
                                        }
                                        else if (frag is AudioFile audioFrag)
                                        {
                                            sb.AppendFormat("\t\t{0}", audioFrag.FileName);
                                            sb.AppendLine();
                                        }


                                    }


                                }
                            }

                        }
                        else
                        {
                            sb.AppendFormat("\t\t{0}", locResp.RepromptTextResponse);
                            sb.AppendLine();
                        }

                    }

                }
            }
            Debug.WriteLine(sb.ToString());

        }

        [Trait("Category", "Serialization")]
        [Fact(DisplayName = "Get Eye of the Elder Gods Test")]
        public void GeEyeOfTheElderGodsTest()
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();


            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("eyeoftheeldergods", "1.0");

            _ = titleReader.GetByIdAsync(titleVer).Result;
        }


        [Fact(DisplayName = "Get Animal Farm PI Test")]
        public void GetAnimalFarmPiTest()
        {

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("animalfarmpi", "1.0");

            _ = titleReader.GetByIdAsync(titleVer).Result;
        }

        [Trait("Category", "Serialization")]
        [Fact(DisplayName = "Assign Phone Number Test")]
        public void AssignPhoneNumberTest()
        {


            IFileRepository fileRep = Services.GetService<S3FileStore>();

            var memDict = GetMemoryCache();

            ILogger<TitleCacheRepository> titleLogger = Services.GetService<ILogger<TitleCacheRepository>>();


            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ITitleCacheRepository titleCacheRep = new TitleCacheRepository(fileRep, memDict, memCache, titleLogger);



            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("personaldatachecker", "1.0");


            var perDataChecker = titleReader.GetByIdAsync(titleVer).Result;

            var welcomeNode = perDataChecker.Nodes.FirstOrDefault(x => x.Name.Equals("WelcomeNewUser"));


            welcomeNode.Actions = new List<NodeActionData>();

            AssignSlotValueActionData assignAction = new AssignSlotValueActionData
            {
                SlotName = "phonenumber",

                Value = "267-555-1212"
            };

            welcomeNode.Actions.Add(assignAction);


            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(perDataChecker);
        }


        [Trait("Category", "Serialization")]
        [Fact(DisplayName = "Get Personal Data Test")]
        public void GetPersonalDataTest()
        {

            IFileRepository fileRep = Services.GetService<S3FileStore>();

            var memDict = GetMemoryCache();

            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();


            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("personaldatachecker", "1.0");


            var perDataChecker = titleReader.GetByIdAsync(titleVer).Result;

            var welcomeNode = perDataChecker.Nodes.FirstOrDefault(x => x.Name.Equals("WelcomeNewUser"));


            welcomeNode.Actions = new List<NodeActionData>();

            GetPersonalInfoActionData personalDataAction = new GetPersonalInfoActionData
            {

                //personalDataAction.PersonalDataType = PersonalDataType.PostalCode;

                SlotName = "postalcode"
            };


            welcomeNode.Actions.Add(personalDataAction);


            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(perDataChecker);
        }

        [Fact(DisplayName = "Load Whetstone skill")]
        public void LoadWhetstoneYaml()
        {
            string text = File.ReadAllText("ImportFiles/whetstonetechnologies/0.1/whetstonetechnologies.yaml");


            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            //WelcomeNewUser
            try
            {
                string welcomeNodeName = retTitle.NewUserNodeName;
                StoryNode welcomeNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals(welcomeNodeName));
                welcomeNode.Actions = new List<NodeActionData>();

                ResetStateActionData resetData = new ResetStateActionData();
                welcomeNode.Actions.Add(resetData);


                var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
                string yamlOut = yamlSer.Serialize(retTitle);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


        }



        [Fact(DisplayName = "Load EOTEG Welcome Node")]
        public void LoadEOTEGWelcomeNodeRequest()
        {
            string text = File.ReadAllText("ImportFiles/eyeoftheeldergods/eyeoftheeldergods.yaml");


            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);


            //WelcomeNewUser
            try
            {
                string welcomeNodeName = retTitle.NewUserNodeName;
                StoryNode welcomeNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals(welcomeNodeName));


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }


        }

        [Fact(DisplayName = "Load Launch Request")]
        public void LoadLaunchRequest()
        {
            string text = File.ReadAllText("samplemessages/launchrequest.json");

            AlexaRequest req = null;

            try
            {
                req = JsonConvert.DeserializeObject<AlexaRequest>(text);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [Fact(DisplayName = "AddRecordedSelection")]
        public async Task AddRecordedItemAction()
        {
            var memDict = GetMemoryCache();

            IMemoryCache memCache = Services.GetService<IMemoryCache>();

            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();


            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVer = new TitleVersion("clinicaltrialsgov", "1.0");


            var clinicalTrial = await titleReader.GetByIdAsync(titleVer);

            var returnNode = clinicalTrial.Nodes.FirstOrDefault(x => x.Name.Equals("ReturningUser"));

            foreach (var nodeAction in returnNode.Actions)
            {
                if (nodeAction is RecordSelectedItemActionData recordAction)
                {
                    recordAction.IsPermanent = false;
                    recordAction.SlotNames = new List<string>
                    {
                        "city",
                        "condition"
                    };

                }

            }



            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(clinicalTrial);
        }

        //symbicordsmsinbound

        [Fact(DisplayName = "Get Discount Test")]
        public void GetDiscountTest()
        {
            ITitleCacheRepository titleCacheRep = Services.GetService<TitleCacheRepository>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVer = new TitleVersion("revenoxdev", "1.0");

            var revenoxTitle = titleReader.GetByIdAsync(titleVer).Result;

            revenoxTitle.PhoneInfo = new StoryPhoneInfo
            {
                SourcePhone = "+17344283758",
                SmsService = SmsSenderType.Pinpoint
            };

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(revenoxTitle);

            using (System.IO.StreamWriter writer = System.IO.File.CreateText("testfile.yaml"))
            {
                writer.Write(yamlOut);
            }
        }


        [Fact(DisplayName = "Get Story Title Yaml to JSON")]
        public void GetStoryNodeJson()
        {
            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVer = new TitleVersion("test_adventure", "1.0");

            var adventureTitle = titleReader.GetByIdAsync(titleVer).Result;

            string adventureJson = JsonConvert.SerializeObject(adventureTitle, Formatting.Indented);
        }


        [Fact(DisplayName = "Get Eye of the elder gods Yaml")]
        public void GetElderGodsTitle()
        {
            ITitleCacheRepository titleCacheRep = Services.GetService<TitleCacheRepository>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVer = new TitleVersion("eyeoftheeldergods", "0.8");

            var adventureTitle = titleReader.GetByIdAsync(titleVer).Result;

            var a1Node = adventureTitle.Nodes.FirstOrDefault(x => x.Name.Equals("A1"));

            a1Node.Actions = new List<NodeActionData>();
            a1Node.AuditBehavior = AuditBehavior.RecordNone;

            ResetStateActionData resetAction = new ResetStateActionData();
            a1Node.Actions.Add(resetAction);

            //a6Node.Actions = new List<NodeActionBase>();

            //a6Node.Actions.Add(visitAction);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string yamlOut = yamlSer.Serialize(adventureTitle);
        }

        [Fact(DisplayName = "Get Animal Farm Title Yaml")]
        public void GetAnimalFarmTitle()
        {
            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVersion = new TitleVersion("animalfarmpi", "1.0");


            var adventureTitle = titleReader.GetByIdAsync(titleVersion).Result;


            JsonSerializerSettings ser = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            string animalFarmJson = JsonConvert.SerializeObject(adventureTitle, ser);
        }

        [Fact(DisplayName = "Get Story Title Yaml")]
        public void GetStoryTitleYamlFile()
        {


            ITitleCacheRepository titleCacheRep = GetTitleCacheRepository();


            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);

            TitleVersion titleVersion = new TitleVersion("animalfarmpi", "1.1");

            var adventureTitle = titleReader.GetByIdAsync(titleVersion).Result;

        }




        [Fact(DisplayName = "Get Eye of the Elder Gods Yaml")]
        public void GetEOTEGYamlFile()
        {
            ITitleCacheRepository titleCacheRep = Services.GetService<TitleCacheRepository>();

            ILogger<YamlTitleReader> yamlLogger = Services.GetService<ILogger<YamlTitleReader>>();

            ITitleReader titleReader = new YamlTitleReader(titleCacheRep, yamlLogger);


            TitleVersion titleVersion = new TitleVersion("eyeoftheeldergods", "1.0");

            var adventureTitle = titleReader.GetByIdAsync(titleVersion).Result;

        }




        [Fact(DisplayName = "Import Title Zip to S3")]
        public async Task ImportStoryTitleZipYaml()
        {


            string testDataFile = string.Concat(@"ImportFiles\", "test_adventure.zip");

            byte[] importZip = File.ReadAllBytes(testDataFile);

            IFileRepository fileRep = Services.GetService<S3FileStore>();

            IStoryTitleImporter storyImporter = new S3StoryTitleImporter(fileRep);

            await storyImporter.ImportFromZipAsync(importZip);

        }

        [Fact]
        public void AddCardResponses()
        {
            string text = File.ReadAllText("importfiles/whetstonetechnologies/0.3/whetstonetechnologies.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            StoryNode editNode = null;

            foreach (StoryNode node in retTitle.Nodes)
            {
                if (String.Compare(node.Name, "SonibridgeInfo", true) == 0)
                {
                    editNode = node;
                    break;
                }
            }


            CardResponse bixbyCard = new CardResponse();

            LocalizedResponse localizedResponse = editNode.ResponseSet[0].LocalizedResponses[0];
            localizedResponse.CardResponses = new List<CardResponse>();

            bixbyCard.SpeechClient = Client.Bixby;
            bixbyCard.LargeImageFile = "foo-large.bar";
            bixbyCard.SmallImageFile = "foo-small.bar";
            bixbyCard.TextFragments = new List<TextFragmentBase>();

            SimpleTextFragment fragment = new SimpleTextFragment
            {
                Text = "Hello weird"
            };
            bixbyCard.TextFragments.Add(fragment);
            bixbyCard.CardTitle = localizedResponse.CardTitle;

            localizedResponse.CardResponses.Add(bixbyCard);

            if (localizedResponse.TextFragments != null)
            {
                CardResponse defaultCard = new CardResponse
                {
                    LargeImageFile = "foo-large.bar",
                    SmallImageFile = "foo-small.bar",
                    TextFragments = localizedResponse.TextFragments,

                    CardTitle = localizedResponse.CardTitle
                };

                localizedResponse.CardResponses.Add(defaultCard);

                localizedResponse.TextFragments = null;
            }

            localizedResponse.CardTitle = null;

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string serializedWhetstone = yamlSer.Serialize(retTitle);
            StoryTitle validateTitle = yamlDeserializer.Deserialize<StoryTitle>(serializedWhetstone);

            StoryNode validateNode = null;

            foreach (StoryNode node in validateTitle.Nodes)
            {
                if (String.Compare(node.Name, "SonibridgeInfo", true) == 0)
                {
                    validateNode = node;
                    break;
                }
            }

            LocalizedResponse validateResponse = validateNode.ResponseSet[0].LocalizedResponses[0];

            if (validateResponse.CardResponses.Count != localizedResponse.CardResponses.Count)
                throw new Exception("Card Response mismatch on deserialization");

        }




    }
}