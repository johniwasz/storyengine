using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Integration
{

    /// <summary>
    /// Used by the story processor to drive logic resulting from a action results.
    /// </summary>
    /// <remarks>For example, if the search result returns more than one record, then the engine behavoir session management is affected.</remarks>
    public abstract class DataRetrievalResultBase
    {

        [DataMember]
        public bool HasNextResult { get; set; }
    }
}
