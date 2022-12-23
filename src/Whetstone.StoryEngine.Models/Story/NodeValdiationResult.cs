using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Whetstone.StoryEngine.Models.Story
{

    [DebuggerDisplay("{NodeName}")]
    public class NodeValdiationResult
    {
        public string NodeName { get; set; }

        public List<string> Messages { get; set; }

    }
}
