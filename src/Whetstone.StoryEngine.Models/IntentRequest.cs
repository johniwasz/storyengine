using System.Collections.Generic;

namespace Whetstone.StoryEngine.Models
{
    public class IntentRequest
    {

        public IntentRequest()
        {

            this.Slots = null;
        }

        public IntentRequest(string intent)
        {
            this.Intent = intent;
            this.Slots = null;

        }


        public string Intent { get; set; }


        public Dictionary<string, string> Slots { get; set; }
    }
}
