using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Integration
{


    /// <summary>
    /// This defines how to call the serverless function
    /// </summary>
    /// <remarks>
    /// This invokes a search request to a serverless function. It passes the user tracking data which will include slot values based on the SelectedItem action. Since there could be 
    /// more than one action, the result set is named.
    /// </remarks>
    [JsonObject]
    [DataContract]
    [MessageObject]
    public class TableFunctionSearchAction : DataRetrievalAction
    {


        public TableFunctionSearchAction()
        {

            DataRetrievalType = DataRetrievalType.TableSearch;
        }

        /// <summary>
        /// The name of serverless function that honors a search request.
        /// </summary>
        [JsonRequired]
        [Required]
        [YamlMember]
        [DataMember]
        public string FunctionName { get; set; }

        
        /// <summary>
        /// The name to reference in speech fragments when processing the search result.
        /// </summary>
        /// <remarks>
        /// Since there could be more than one search result to process, this uniquely identifies the returned result.
        /// </remarsks>
        [YamlMember]
        [DataMember]
        public string ResultSetName { get; set; }



        /// <summary>
        /// Alias of the lambda function to invoke.
        /// </summary>
        [YamlMember]
        [DataMember]
        public string Alias { get; set; }


        [JsonRequired]
        [YamlIgnore]
        [DataMember]
        public override DataRetrievalType DataRetrievalType { get; set; }

        [YamlMember]
        [DataMember]
        public override bool? CacheResult { get; set; }
    }
}
