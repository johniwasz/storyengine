using Sbs.StoryEngine.Models.Story;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Sbs.StoryEngine.Models;

namespace Sbs.StoryEngine.Repository
{
    internal static class StoryExtensions
    {

        public static string GetChoiceNodeId(this IChoice selectedChoice)
        {

            SingleNodeChoice singleChoice = selectedChoice as SingleNodeChoice;

            if (singleChoice != null)
                return singleChoice.IntentName;
            else
            {

                MultiNodeChoice multiNodeChoice = selectedChoice as MultiNodeChoice;

                var multiNodeChoices = multiNodeChoice.StoryNodeNames;

                int index = GetRandomIndex(multiNodeChoices.Count);
                return multiNodeChoices[index];

            }
        }


        public static string GetChoiceNodeName(this IChoice selectedChoice)
        {

            SingleNodeChoice singleChoice = selectedChoice as SingleNodeChoice;

            if (singleChoice != null)
                return singleChoice.StoryNodeName;
            else
            {

                MultiNodeChoice multiNodeChoice = selectedChoice as MultiNodeChoice;

                var multiNodeChoices = multiNodeChoice.StoryNodeNames;

                int index = GetRandomIndex(multiNodeChoices.Count);
                return multiNodeChoices[index];

            }
        }

        public static StoryResponse ToStoryResponse(this StoryNode node)
        {
            StoryResponse result = new StoryResponse();


            result.TitleId = node.TitleId;
            result.Response = node.GetResponse();
            result.Choices = node.Choices;
            result.NodeName = node.Name;
            result.SmallImageFile = node.SmallImageFile;
            result.LargeImageFile = node.LargeImageFile;
            result.Actions = node.Actions;
            return result;


        }

        public static LocalizedResponse GetResponse(this StoryNode node)
        {
            LocalizedResponse returnResponse = null;
            List<LocalizedResponse> selectedResponses = null;


            switch(node.ResponseBehavior)
            {
                case ResponseBehavior.SelectFirst:
                    selectedResponses = node.ResponseSet.FirstOrDefault();

                    break;
                case ResponseBehavior.Random:
                    int index = GetRandomIndex(node.ResponseSet.Count);
                    selectedResponses = node.ResponseSet[index];
                    break;
            }

            // TODO -- pick the response based on the locale. For now just pick the top one.
            returnResponse = selectedResponses.FirstOrDefault();
            
            return returnResponse;
        }


        private static int GetRandomIndex(int upperIndex)
        {

            Random r = new Random();
            int pickedIndex = r.Next(0, upperIndex);


            return pickedIndex;
        }

    }


   
}
