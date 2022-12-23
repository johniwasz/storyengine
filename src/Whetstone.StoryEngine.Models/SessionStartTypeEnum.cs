using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models
{
    /// <summary>
    /// Indicates how the user started the skill. They can start by requesting a launch operation ("Alexa, start animal farm pi") or from a direct intent ("Alexa, ask clinical trial finder for lung cancer trials in Boston")
    /// </summary>
    public enum SessionStartType
    {
        /// <summary>
        /// This indicates that the user started the session from a launch request (e.g. "Alexa, open clinical trial finder")
        /// </summary>
        LaunchStart,
        /// <summary>
        /// The session started from a permissible intent and not from a launch request (e.g. "Alexa, ask clinical trial finder for lung cancer trials in Boston")
        /// </summary>
        IntentStart

    }
}
