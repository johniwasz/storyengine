using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.AlexaProcessor.Configuration
{
    public class AlexaConfig
    {

        /// <summary>
        /// Indicates whether or not to check the signature of the Alexa request. This should be true in production and
        /// false in development.
        /// </summary>
        public bool EnforceAlexaPolicyCheck { get; set; }



        /// <summary>
        /// Indicates whether Alexa messages should be recorded
        /// </summary>
        public bool AuditClientMessages { get; set; }
    }
}
