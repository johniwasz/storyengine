using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{



    public class ReportGeneratorConfig
    {
        [JsonProperty(PropertyName = "reportBucket")]
        [YamlMember(Alias = "reportBucket")]
        public string ReportBucket { get; set; }

        [JsonProperty(PropertyName = "reportStepFunctionArn")]
        [YamlMember(Alias = "reportStepFunctionArn")]
        public string ReportStepFunctionArn { get; set; }
    }
}
