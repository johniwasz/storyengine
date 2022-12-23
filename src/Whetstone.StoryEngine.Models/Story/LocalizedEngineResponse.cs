using System;
using System.Collections.Generic;
using System.Text;
using Whetstone.StoryEngine.Models.Story.Cards;
using Whetstone.StoryEngine.Models.Story.Ssml;
using Whetstone.StoryEngine.Models.Story.Text;

namespace Whetstone.StoryEngine.Models.Story
{

    /// <summary>
    /// This is the response returned once the Story Engine has 
    /// determined the response to return. 
    /// </summary>
    /// <remarks>
    /// This should not be conflated with the Story definition LocalizedResponse
    /// </remarks>
    public class LocalizedEngineResponse
    {

        public CardEngineResponse CardResponse { get; set; }

        public List<string> RepromptTextResponses { get; set; }


        /// <summary>
        /// This is the standard SSML response for audio appliances like Alexa and Google Home.
        /// </summary>
        /// <remarks>If this is null then the text response is returned and 
        /// just wrapped in a speak tag to be SSML compliant.</remarks>
        public List<SpeechFragment> SpeechResponses { get; set; }

        public List<SpeechFragment> RepromptSpeechResponses { get; set; }


        public List<TextFragmentBase> GeneratedTextResponse { get; set; }


    }
}
