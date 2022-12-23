using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.UnitTests
{
    public class ExpectedNodeResult
    {

        public ExpectedNodeResult()
        {
            this.HasCardText = true;
        }

        public string NodeName { get; set; }

        public bool HasCardResponse { get; set; }

        public bool HasCardButtons { get; set; }

        public bool HasCardText { get; set; }
    }
}
