using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Reporting.Models
{

    /// <summary>
    /// Request to dispatch a scheduled report.
    /// </summary>
    public class ReportRequest
    {

        /// <summary>
        /// This is the name of the report to generate. It maps to the report format and source data.
        /// </summary>
        /// <remarks>
        /// This value maps to the report definition which states which query or function to invoke as well
        /// as the output format of the report and the destination to send the report to.
        /// </remarks>
        [JsonProperty(PropertyName = "reportName", Required = Required.Always)]
        public string ReportName { get; set; }


        /// <summary>
        /// Parameter values to map to the query that generates the report.
        /// </summary>
        [JsonProperty(PropertyName = "parameters")]
        public Dictionary<string, dynamic> Parameters { get; set; }

        /// <summary>
        /// If missing, the report is sent immediately. If set, then the report is sent on or shortly after
        /// the specified date and time. The date must be in the future.
        /// </summary>
        [JsonProperty(PropertyName = "deliveryTime",NullValueHandling =  NullValueHandling.Ignore)]
        public DateTime? DeliveryTime { get; set; }


        /// <summary>
        /// If true, then the DeliveryType should not be null.
        /// </summary>
        /// <remarks>
        /// This is required so that the step function can use a field for decisioning logic. There is no logic
        /// for a check for null.
        /// </remarks>
        [JsonProperty(PropertyName = "isScheduled", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsScheduled { get; set; }
    }
}
