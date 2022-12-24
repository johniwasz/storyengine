using System.Collections.Generic;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;
using Xunit;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class AdventureTest : LambdaTestBase
    {


        [Fact(DisplayName = "Create Title Story")]
        public void CreateTitle()
        {

            StoryTitle newTitle = new StoryTitle();
            newTitle.Id = "test_title";
            newTitle.Description = "Sample for serialization";

            newTitle.Title = "Serialization Title";

            newTitle.Nodes = new List<StoryNode>();

            StoryNode node = new StoryNode();

            node.Name = "GoodTea";
            node.Choices = new List<Choice>();

            node.ResponseSet = new List<LocalizedResponseSet>();


            LocalizedResponse locResponse = new LocalizedResponse();
            locResponse.CardTitle = "He Like Tea";
            locResponse.SendCardResponse = true;
            locResponse.TextFragments = new List<TextFragmentBase>();

            locResponse.TextFragments.Add(new SimpleTextFragment("He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation. You can talk to the troll or keep walking."));
            ;
            locResponse.RepromptSpeechResponses = new List<ClientSpeechFragments>();
            locResponse.RepromptSpeechResponses.Add(new ClientSpeechFragments());
            //  locResponse.RepromptSpeechResponses[0].SpeechClient = Client.Alexa;
            locResponse.RepromptSpeechResponses[0].SpeechFragments = new List<SpeechFragment>();
            locResponse.RepromptSpeechResponses[0].SpeechFragments.Add(new PlainTextSpeechFragment() { Text = "Would you like to talk to the troll or keep walking?" });

            locResponse.SpeechResponses = new List<ClientSpeechFragments>();
            locResponse.SpeechResponses.Add(new ClientSpeechFragments());
            //locResponse.SpeechResponses[0].SpeechClient = Client.Alexa;
            locResponse.SpeechResponses[0].SpeechFragments = new List<SpeechFragment>();
            locResponse.SpeechResponses[0].SpeechFragments.Add(new PlainTextSpeechFragment() { Text = "He takes a sip and seems happy with the tea. He smiles. He looks like he would enjoy a conversation. You can talk to the troll or keep walking." });


            NodeVisitCondition visitCondition = new NodeVisitCondition();
            string punchCondition = "PunchedTroll";
            visitCondition.Name = punchCondition;
            visitCondition.RequiredNodes = new List<string>();
            visitCondition.RequiredNodes.Add("Troll Laughs");

            newTitle.Conditions = new List<StoryConditionBase>();
            newTitle.Conditions.Add(visitCondition);

            LocalizedResponseSet localizedResponseSet = new LocalizedResponseSet();
            localizedResponseSet.LocalizedResponses = new List<LocalizedResponse>();


            node.ResponseSet.Add(localizedResponseSet);
            node.ResponseSet[0].LocalizedResponses.Add(locResponse);

            node.Choices = new List<Choice>();

            Choice walkChoice = new Choice();
            walkChoice.IntentName = "WalkIntent";
            walkChoice.NodeMapping = new SingleNodeMapping() { NodeName = "Out of the Woods" };
            node.Choices.Add(walkChoice);

            Choice talkChoice = new Choice();
            talkChoice.IntentName = "TalkToTrollIntent";
            talkChoice.NodeMapping = new SingleNodeMapping() { NodeName = "Talk to Troll" };
            node.Choices.Add(talkChoice);

            Choice flyChoice = new Choice();
            flyChoice.IntentName = "FlyIntent";

            ConditionalNodeMapping conditionalMapping = new ConditionalNodeMapping();
            conditionalMapping.Conditions = new List<string>();
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
