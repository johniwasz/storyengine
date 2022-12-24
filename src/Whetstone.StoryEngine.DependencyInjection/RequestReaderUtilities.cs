using System;
using Whetstone.StoryEngine.Models;

namespace Whetstone.StoryEngine.DependencyInjection
{
    public static class RequestReaderUtilities
    {

        /// <summary>
        /// Maps the standard intent to the request type.
        /// </summary>
        /// <param name="intentText">The intent text.</param>
        /// <returns></returns>
        public static StoryRequestType GetRequestType(string intentText)
        {
            StoryRequestType returnType = StoryRequestType.Intent;


            if (intentText.Equals(ReservedIntents.BeingIntent.Name, StringComparison.OrdinalIgnoreCase) ||
                intentText.Equals(ReservedIntents.RestartIntent.Name, StringComparison.OrdinalIgnoreCase) ||
                intentText.Equals("StartInvestigationIntent", StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Begin;
            else if (intentText.Equals(ReservedIntents.RepeatIntent.Name, StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Repeat;
            else if (intentText.Equals(ReservedIntents.PauseIntent.Name, StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Pause;
            else if (intentText.Equals(ReservedIntents.ResumeIntent.Name, StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Resume;
            else if (intentText.Equals(ReservedIntents.EndGameIntent.Name, StringComparison.OrdinalIgnoreCase) ||
                     intentText.Equals(ReservedIntents.StopIntent.Name, StringComparison.OrdinalIgnoreCase) ||
                      intentText.Equals(ReservedIntents.CancelIntent.Name, StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Stop;
            else if (intentText.Equals(ReservedIntents.HelpIntent.Name, StringComparison.OrdinalIgnoreCase))
                returnType = StoryRequestType.Help;
            else
                returnType = StoryRequestType.Intent;


            return returnType;
        }

    }
}
