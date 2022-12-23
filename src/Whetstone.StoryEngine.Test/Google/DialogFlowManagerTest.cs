using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Whetstone.StoryEngine.Google.Management;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Data.Amazon;
using Whetstone.StoryEngine.Data.Yaml;
using Whetstone.StoryEngine.Models.Story;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Whetstone.StoryEngine.Models.Tracking;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Whetstone.StoryEngine.Test.Google
{

    public struct UtterancePart
    {
        public string Text;
        public string SlotName;

    }



    public class DialogFlowManagerTest : TestServerFixture
    {


        [Fact]
        public void RegTokenTest()
        {



            string utterance = "{verb} {item}";
            

            List<UtterancePart> parseUtterances = ParseUtterance(utterance);

            List<UtterancePart> utteranceParts = new List<UtterancePart>();

            SlotType verbSlots = new SlotType();
            verbSlots.Name= "verb";
            verbSlots.Values = new List<SlotValue>();
            verbSlots.Values.Add(new SlotValue() { Value = "home" });
            verbSlots.Values.Add(new SlotValue() { Value = "door" });
            verbSlots.Values.Add(new SlotValue() { Value = "book" });
            verbSlots.Values.Add(new SlotValue() { Value = "ball" });
            

            SlotType itemSlots = new SlotType();
            itemSlots.Name = "item";
            itemSlots.Values = new List<SlotValue>();
            itemSlots.Values.Add(new SlotValue() { Value =  "home" });
            itemSlots.Values.Add(new SlotValue() { Value = "door" });
            itemSlots.Values.Add(new SlotValue() { Value = "book" });
            itemSlots.Values.Add(new SlotValue() { Value = "ball" });


            List<string> combination = new List<string>();

            List<SlotType> allSlots = new List<SlotType>();
            allSlots.Add(verbSlots);
            allSlots.Add(itemSlots);

            AddCombinations(combination, allSlots, 0, utterance);

        }


        private List<UtterancePart> ParseUtterance(string utterance)
        {
            List<UtterancePart> utteranceParts = new List<UtterancePart>();

            bool isInBrackets = false;

            UtterancePart curPart = default(UtterancePart);
            for (int i = 0; i < utterance.Length; i++)
            {
                char curChar = utterance[i];

                if (curChar == '{')
                {
                    isInBrackets = true;

                    if (!string.IsNullOrWhiteSpace(curPart.SlotName) || curPart.Text?.Length>0)
                        utteranceParts.Add(curPart);

                    i++;
                    if (i < utterance.Length)
                    {
                        curChar = utterance[i];
                        curPart = new UtterancePart();
                    }
                }
                else if (curChar == '}')
                {
                    isInBrackets = false;
                    utteranceParts.Add(curPart);
                    i++;
                    if (i < utterance.Length)
                    {
                        curChar = utterance[i];
                        curPart = new UtterancePart();
                    }
                }


                if (isInBrackets)
                {
                    if (curPart.SlotName == null)
                    {
                        curPart.SlotName = "";
                    }
                    curPart.SlotName += curChar;

                }
                else
                {
                    if (curPart.Text == null)
                    {
                        curPart.Text = "";
                    }

                    if(curChar!='}')
                        curPart.Text += curChar;
                }

            }

            if (curPart.Text!=null && curPart.Text.Length>0)
            {
                utteranceParts.Add(curPart);
            }
            return utteranceParts;
        }

        static void AddCombinations(List<string> result, List<SlotType> levels, int level, string path)
        {
            if (level >= levels.Count)
            {
                result.Add(path);
                return;
            }

            SlotType curSlot = levels[level];


            foreach (var item in curSlot.Values)
            {
                string pathVal = null;

                if(path!=null)
                {
                    pathVal = path.Replace(string.Concat("{", curSlot.Name, "}"),
                        string.Concat("{", curSlot.Name, ":", item.Value, "}"));
                    
                }

                AddCombinations(result, levels, level + 1, pathVal);
            }
        }

        [Fact]
        public async Task ImportElderGodsTest()
        {
            string titleId = "eyeoftheeldergods";
            TitleVersion titleVer = new TitleVersion(titleId, "1.0");
            string projectId = "eyeoftheeldergods-11112";

            StoryTitle title = await GetTitleAsync(titleVer);
            var logger = CreateLogger<DialogFlowManager>();


            IDialogFlowManager dialogFlowManager = new DialogFlowManager(logger);

            await dialogFlowManager.ImportTitleAsync(projectId, title);

        }


        [Fact]
        public async Task ImportWhetstoneTechnologiesTest()
        {

            var googleAppCred = System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");



            string titleId = "whetstonetechnologies";
            TitleVersion titleVer = new TitleVersion(titleId, "0.1");

            var titleReader = this.Services.GetRequiredService<ITitleReader>();

            StoryTitle title = await titleReader.GetByIdAsync(titleVer);


            string projectId = "whetstone-technologies";
            ILogger<DialogFlowManager> logger = CreateLogger<DialogFlowManager>();

            IDialogFlowManager dialogFlowManager = new DialogFlowManager(logger);

            await dialogFlowManager.ImportTitleAsync(projectId, title);

        }

        [Fact]
        public async Task ImportStatelineTest()
        {
            //gcloud config set project statilean - savings - action - efafe45518f1

            var googleAppCred =  System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            string titleId = "statileansavings";

            string projectId = "statilean-savings-action";

            TitleVersion titleVer = new TitleVersion(titleId, "1.0");

            StoryTitle title = await GetTitleAsync(titleVer);
            var logger = CreateLogger<DialogFlowManager>();

            IDialogFlowManager dialogFlowManager = new DialogFlowManager(logger);

            await dialogFlowManager.ImportTitleAsync(projectId, title);

        }


        [Fact]
        public async Task ImportAnimalFarmPI()
        {
            //gcloud config set project statilean - savings - action - efafe45518f1

            var googleAppCred = System.Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");

            string titleId = "animalfarmpi";

            string projectId = "animal-farm-pi";


            TitleVersion titleVer = new TitleVersion(titleId, "1.0");
            StoryTitle title = await GetTitleAsync( titleVer);
            var logger = CreateLogger<DialogFlowManager>();

            IDialogFlowManager dialogFlowManager = new DialogFlowManager(logger);

            await dialogFlowManager.ImportTitleAsync(projectId, title);

        }



        



        [Fact]
        public void ManageTestAsync()
        {


            DialogFlowConnector.ListIntentsAsync();

        }

        [Fact]
        public async Task DynaExec()
        {
            string replacementText = "I heard you say <say-as interpret-as=\"telephone\">@@String.Format(\"{0:###-###-####}\", {phonenumber})@@</say-as>.";

     
            SelectedItem selItem = new SelectedItem();

            selItem.Name = "phonenumber";
            selItem.Value = "2675551212";

            List<SelectedItem> selItems = new List<SelectedItem>() { selItem };

            ILogger curLogger = CreateLogger<DialogFlowManager>();



            string outText = await MacroProcessing.ProcessTextFragmentMacrosAsync(replacementText, selItems, curLogger);

            string regExpression = @"@@(.*?)@@";
            var matchCol = Regex.Matches(outText, regExpression);


            foreach (Match mat in matchCol)
            {
                Debug.WriteLine(mat.Value);

                string codeText = mat.Value.Substring(2, mat.Value.Length - 4);

                string evaluatedText = await CSharpScript.EvaluateAsync<string>(codeText);

                outText = outText.Replace(mat.Value, evaluatedText);


            }


            

        }

    }


    
}
