using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Whetstone.StoryEngine.Reporting.Models.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{

    public enum ReportDestinationType
    {
        SftpEndpoint = 1
        // Could add email and others.

    }

    /// <summary>
    /// Defines where to send the report.
    /// </summary>
    [JsonConverter(typeof(ReportDestinationTypeConverter))]
    public abstract class ReportDestinationBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "destinationType")]
        public abstract ReportDestinationType DestinationType { get; set; }





    }


}
