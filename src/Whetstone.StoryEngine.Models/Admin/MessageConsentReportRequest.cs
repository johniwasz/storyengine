using Newtonsoft.Json;
using System;

namespace Whetstone.StoryEngine.Models.Admin
{

    /// <summary>
    /// Use to request message consent export CSV file.
    /// </summary>
    /// <remarks></remarks>
    public class MessageConsentReportRequest
    {
        [JsonProperty(PropertyName = "titleId", Required = Required.Always)]
        public Guid TitleId { get; set; }


        [JsonProperty(PropertyName = "startTime", Required = Required.Always)]
        public DateTime StartTime { get; set; }


        [JsonProperty(PropertyName = "endTime", Required = Required.Always)]
        public DateTime EndTime { get; set; }
    }
}
