using System;
using System.Collections.Generic;
using System.Text;
using Amazon.Runtime.Internal.Transform;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Reporting.Models
{
    public class ReportMetadata
    {

        [JsonProperty(PropertyName = "rowCount")]
        public int RowCount { get; set; }


        [JsonProperty(PropertyName = "dateGenerated")]
        public DateTime DateGenerated { get; set; }


        /// <summary>
        /// Parameter values to map to the query that generates the report.
        /// </summary>
        [JsonProperty(PropertyName = "parameters")]
        public Dictionary<string, dynamic> Parameters { get; set; }

        /// <summary>
        /// If missing, the report is sent immediately. If set, then the report is sent on or shortly after
        /// the specified date and time. The date must be in the future.
        /// </summary>
        [JsonProperty(PropertyName = "deliveryTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime DeliveryTime { get; set; }
    }
}
