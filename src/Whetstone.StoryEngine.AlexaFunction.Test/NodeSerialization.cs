using Whetstone.StoryEngine.Models.Actions;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Models.Tracking;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Whetstone.StoryEngine.Models.Conditions;

namespace Whetstone.StoryEngine.AlexaFunction.Test
{
    public class NodeSerialization : LambdaTestBase
    {

        [Fact]
        public void EOTEGGoogleClientAddTest()
        {
            string elderGodsTest = File.ReadAllText(@"stories/eyeoftheeldergods/eyeoftheeldergods.yaml");

            var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();


            StoryTitle eotegTitle = yamlDeser.Deserialize<StoryTitle>(elderGodsTest);

            // Add restart to C20b, revisit C5, D12a needs a restart choice, D16a, D19a need a restart choice
            // D20ab - restart or end game
            // D23ab - check choice selection
            // D24ab - add restart 

            StoryNode d24ab = eotegTitle.Nodes.FirstOrDefault(x => x.Name.Equals("D24ab"));

            d24ab.LocalizedSuggestions = new List<List<LocalizedPlainText>>();

            List<LocalizedPlainText> locPlainTexts = new List<LocalizedPlainText>();
            locPlainTexts.Add(new LocalizedPlainText() { Text = "restart" });
            d24ab.LocalizedSuggestions.Add(locPlainTexts);



            //Choice goBackChoice = new Choice();
            //goBackChoice.IntentName = "GoBackIntent";
            //ConditionalNodeMapping condNodeMapping = new ConditionalNodeMapping();
            //condNodeMapping.Conditions = new List<string>();
            //condNodeMapping.Conditions.Add("BookOrKey");

            //SingleNodeMapping trueMapping = new SingleNodeMapping("D24");
            //trueMapping.LocalizedSuggestionText = new List<LocalizedPlainText>();
            //trueMapping.LocalizedSuggestionText.Add(new LocalizedPlainText() { Text = "go back" });
            //condNodeMapping.TrueConditionResult = trueMapping;

            //SingleNodeMapping falseMapping = new SingleNodeMapping("D27");
            //falseMapping.LocalizedSuggestionText = new List<LocalizedPlainText>();
            //falseMapping.LocalizedSuggestionText.Add(new LocalizedPlainText() { Text = "go back" });
            //condNodeMapping.FalseConditionResult = falseMapping;

            //goBackChoice.NodeMapping = condNodeMapping;

            //bookOrKey.Choices.Add(goBackChoice);

            var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            string yamlText = yamlSer.Serialize(eotegTitle);



            List<StoryConditionBase> storyConditions = eotegTitle.Conditions;

            List<string> textSuggestions = new List<string>();

            List<IStoryCrumb> crumbs = new List<IStoryCrumb>();

            List<IStoryCrumb> permCrumbs = new List<IStoryCrumb>();

            ConditionInfo conditionInfo = new ConditionInfo(crumbs, permCrumbs, Models.Client.Alexa);

            string locale = "en-US";

            foreach (var node in eotegTitle.Nodes)
            {
                List<string> suggestions = node.GetSuggestions(locale, conditionInfo, storyConditions);
              


                if (suggestions.Any())
                {
                    //foreach (string suggestion in suggestions)
                    //    Debug.WriteLine($"{node.Name}: {suggestion}");
                }
                else
                    Debug.WriteLine($"Empty: {node.Name}");
                

            }

            //var yamlSer = YamlSerializationBuilder.GetYamlSerializer();

            //string yamlText = yamlSer.Serialize(eotegTitle);

        }





        //[Fact(DisplayName="MessagePack Validation")]
        //public void MessagePackValidation()
        //{

        //    StoryTitle title = GetSampleTitle();

        //    byte[] nodeBytes = MessagePack.MessagePackSerializer.Serialize(title, MessagePack.Resolvers.ContractlessStandardResolver.Instance);


        //    StoryTitle titleDeser =  MessagePack.MessagePackSerializer.Deserialize<StoryTitle>(nodeBytes, MessagePack.Resolvers.ContractlessStandardResolver.Instance);


        //}





        //[Fact(DisplayName = "Yaml Validation")]
        //public void YamlValidation()
        //{

        //    StoryTitle title = GetSampleTitle();

        //    var yamlSeserializer = YamlSerializationBuilder.GetYamlSerializer();

        //    var yamlDeser = YamlSerializationBuilder.GetYamlDeserializer();

        //    string storyYaml = yamlSeserializer.Serialize(title);

        //    StoryTitle deserTitle = yamlDeser.Deserialize<StoryTitle>(storyYaml);


        //}

        //private StoryTitle GetSampleTitle()
        //{

        //    StoryTitle title = new StoryTitle();

        //    title.Intents = new List<Intent>();

        //    title.Intents.Add(new Intent() { Name = "TestIntent" });

        //    StoryNode node = new StoryNode();

        //    node.Name = "TestNode";

        //    node.Choices = new List<Choice>();

        //    MultiNodeChoice multiChoice = new MultiNodeChoice();
        //    multiChoice.IntentName = "Multi";
        //    multiChoice.StoryNodeNames = new List<string>();
        //    multiChoice.StoryNodeNames.Add("First Node");
        //    multiChoice.StoryNodeNames.Add("Second Node");
        //    multiChoice.StoryNodeNames.Add("Third Node");

        //    SingleNodeChoice singleChoice = new SingleNodeChoice();
        //    singleChoice.IntentName = "Single Choice";
        //    singleChoice.StoryNodeName = "Single Node";

        //    node.Choices.Add(multiChoice);
        //    node.Choices.Add(singleChoice);


        //    node.Actions = new List<INodeAction>();

        //    InventoryAction addMackrelAction = new InventoryAction();
        //    addMackrelAction.Item = new UniqueItem() { Name = "lollipop" };
        //    node.Actions.Add(addMackrelAction);


        //    NodeVisitRecordAction visitAction = new NodeVisitRecordAction();
        //    node.Actions.Add(visitAction);


        //    NodeVisitRecordAction recordVisit = new NodeVisitRecordAction();



        //    title.Nodes = new List<StoryNode>();
        //    title.Nodes.Add(node);

        //    return title;

        //}

    }
}
