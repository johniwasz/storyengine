using Whetstone.StoryEngine.Models.Story;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Integration
{
    [DataContract]
    public class ExternalFunctionResult : DataRetrievalResultBase
    {

        [DataMember]
        public LocalizedEngineResponse Response { get; set; }

        /// <summary>
        /// Used by the Alexa CanFulfillIntent validation process.
        /// </summary>
        [DataMember]
        public Dictionary<string, SlotCanFulFill> SupportedSlots { get; set; }

    }
}
