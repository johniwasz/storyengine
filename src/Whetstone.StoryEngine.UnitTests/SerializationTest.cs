using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.Alexa;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Xunit;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.UnitTests
{
    public class SerializationTest
    {

        [Theory(DisplayName ="Deserialize title yaml files")]
        [InlineData(@"TitleFiles/whetstonetechnologies/0.1/whetstonetechnologies.yaml")]
       [InlineData(@"TitleFiles/whetstonetechnologies/0.4/whetstonetechnologies.yaml")]
       [InlineData(@"TitleFiles/animalfarmpi/1.2/animalfarmpi.yaml")]
        [InlineData(@"TitleFiles/eyeoftheeldergods/0.8/eyeoftheeldergods.yaml")]
        [InlineData(@"TitleFiles/clinicaltrialsgov/0.1/clinicaltrialsgov.yaml")]
        public void DeserializeTitle(string yamlFilePath)
        {
            string text = File.ReadAllText(yamlFilePath);

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);
            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string plainYamlText = yamlSer.Serialize(retTitle);
        }

        [Fact(DisplayName = "LoadBootstrapConfig")]
        public void DeserializeBootstrapConfig()
        {

            string text = File.ReadAllText(@"TitleFiles/bootstrap.yml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            BootstrapConfig retConfig = yamlDeserializer.Deserialize<BootstrapConfig>(text);
            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();
            string plainYamlText = yamlSer.Serialize(retConfig);

        }

        [Fact(DisplayName = "LoadPlainTextSuggestion")]
        public void CheckPlainTextSuggestrion()
        {
            string text = File.ReadAllText("TitleFiles/whetstonetechnologies/0.4/whetstonetechnologies.yaml");

            var yamlDeserializer = YamlSerializationBuilder.GetYamlDeserializer();
            StoryTitle retTitle = yamlDeserializer.Deserialize<StoryTitle>(text);

            var infoNode = retTitle.Nodes.FirstOrDefault(x => x.Name.Equals("SoniBridgeInfo"));

            var yesChoice = infoNode.Choices.FirstOrDefault(x => x.IntentName.Equals("YesIntent"));

            

            /// SoniBridgeInfo
        }

        [Fact(DisplayName = "LoadAlexaRequest")]
        public void LoadAlexaRequest()
        {

            string text = File.ReadAllText(@"Alexa/newsession.json");

            AlexaRequest req = JsonConvert.DeserializeObject<AlexaRequest>(text);

            var storyRequest = req.ToStoryRequest();



        }



        [Fact(DisplayName = "Create Title Story")]
        public void CreateTitle()
        {

            StoryTitle newTitle = new StoryTitle
            {
                Id = "test_title",
                Description = "Sample for serialization",

                Title = "Serialization Title",

                Nodes = new List<StoryNode>()
            };

            StoryNode node = new StoryNode
            {
                Name = "GoodTea",
                Choices = new List<Choice>(),

                ResponseSet = new List<LocalizedResponseSet>()
            };


            LocalizedResponse locResponse = new LocalizedResponse
            {
                CardTitle = "He Like Tea",
                SendCardResponse = true,
                TextFragments = new List<TextFragmentBase>()
            };

            locResponse.TextFragments.Add(new SimpleTextFragment("He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation. You can talk to the troll or keep walking."));
            ;
            locResponse.RepromptSpeechResponses = new List<ClientSpeechFragments>
            {
                new ClientSpeechFragments()
            };
            //  locResponse.RepromptSpeechResponses[0].SpeechClient = Client.Alexa;
            locResponse.RepromptSpeechResponses[0].SpeechFragments = new List<SpeechFragment>
            {
                new PlainTextSpeechFragment() { Text = "Would you like to talk to the troll or keep walking?" }
            };

            locResponse.SpeechResponses = new List<ClientSpeechFragments>
            {
                new ClientSpeechFragments()
            };
            //locResponse.SpeechResponses[0].SpeechClient = Client.Alexa;
            locResponse.SpeechResponses[0].SpeechFragments = new List<SpeechFragment>
            {
                new PlainTextSpeechFragment() { Text = "He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation. You can talk to the troll or keep walking." }
            };


            NodeVisitCondition visitCondition = new NodeVisitCondition();
            string punchCondition = "PunchedTroll";
            visitCondition.Name = punchCondition;
            visitCondition.RequiredNodes = new List<string>
            {
                "Troll Laughs"
            };

            newTitle.Conditions = new List<StoryConditionBase>
            {
                visitCondition
            };

            LocalizedResponseSet localizedResponseSet = new LocalizedResponseSet
            {
                LocalizedResponses = new List<LocalizedResponse>()
            };


            node.ResponseSet.Add(localizedResponseSet);
            node.ResponseSet[0].LocalizedResponses.Add(locResponse);

            node.Choices = new List<Choice>();

            Choice walkChoice = new Choice
            {
                IntentName = "WalkIntent",
                NodeMapping = new SingleNodeMapping() { NodeName = "Out of the Woods" }
            };

            node.Choices.Add(walkChoice);

            Choice talkChoice = new Choice
            {
                IntentName = "TalkToTrollIntent",
                NodeMapping = new SingleNodeMapping() { NodeName = "Talk to Troll" }
            };
            node.Choices.Add(talkChoice);

            Choice flyChoice = new Choice
            {
                IntentName = "FlyIntent"
            };

            ConditionalNodeMapping conditionalMapping = new ConditionalNodeMapping
            {
                Conditions = new List<string>()
            };
            conditionalMapping.Conditions.Add(punchCondition);

            conditionalMapping.TrueConditionResult = new SingleNodeMapping() { NodeName = "Troll Punches" };

            conditionalMapping.FalseConditionResult = new SingleNodeMapping() { NodeName = "Out of the Woods" };


            flyChoice.NodeMapping = conditionalMapping;
            node.Choices.Add(flyChoice);
            newTitle.Nodes.Add(node);

            var ser = YamlSerializationBuilder.GetYamlSerializer();

            string yaml = ser.Serialize(newTitle);

        }


    }
}
