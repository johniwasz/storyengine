using System.Collections.Generic;

namespace Whetstone.StoryEngine.Models.Story.Cards
{
    public class CardEngineResponse
    {

        public string CardTitle { get; set; }

        /// <summary>
        /// This is the story text that is returned to the client if the client supports text only.
        /// </summary>
        /// <remarks>
        /// If this is coming back to Alexa and the SpeechResponse for Alexa is set, then the SpeechResponse is used.
        /// In Alexa, this text response will always display in a card response.
        /// </remarks>
        public List<string> Text { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        public string SmallImageFile { get; set; }


        /// <summary>
        ///  This image feeds into card responses.
        /// </summary>
        public string LargeImageFile { get; set; }



        /// <summary>
        /// Buttons are only applicable to Google Card responses.
        /// </summary>
        public List<CardButton> Buttons { get; set; }


    }
}
