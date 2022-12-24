using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Actions;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Integration
{

    /// <summary>
    /// Calls a lambda function and retrieves a result set.
    /// </summary>
    /// <remarks>The name of the function must be set.</remarks>
    [JsonObject]
    [DataContract]
    [MessageObject]
    public class ExternalFunctionAction : DataRetrievalAction
    {

        public ExternalFunctionAction()
        {
            DataRetrievalType = DataRetrievalType.ExternalRequest;

        }


        /// <summary>
        /// The name of serverless function that honors a search request.
        /// </summary>
        [JsonRequired]
        [Required]
        [YamlMember]
        [DataMember]
        public string FunctionName { get; set; }

        [DataMember]
        [YamlMember]
        public string Alias { get; set; }

        /// <summary>
        /// Pulls an integer value from a tracking item by name. Integer value expected. Not required.
        /// </summary>
        /// <remarks>
        /// This works with a MultiItem inventory item.
        /// </remarks>
        [YamlMember]
        [DataMember]
        public string IndexItem { get; set; }

        [JsonRequired]
        [YamlIgnore]
        [DataMember]
        public override DataRetrievalType DataRetrievalType { get; set; }

        [DataMember]
        [YamlMember]
        public override bool? CacheResult { get; set; }

    }
}
