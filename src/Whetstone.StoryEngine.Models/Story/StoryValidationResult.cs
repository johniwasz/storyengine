using System.Collections.Generic;

namespace Whetstone.StoryEngine.Models.Story
{
    public class StoryValidationResult
    {

        public StoryValidationResult()
        {
            NodeIssues = new List<NodeValdiationResult>();

        }

        public List<NodeValdiationResult> NodeIssues { get; set; }

        public List<string> UnusedAudioFiles { get; set; }

    }
}
