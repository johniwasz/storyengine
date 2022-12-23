using Newtonsoft.Json;


namespace Whetstone.StoryEngine.Bixby.Repository.Models
{
    public class BixbyCallbackResponse_V1
    {
        [JsonProperty("dlg")]
        public string Dlg { get; set; }

        [JsonProperty("hasCard")]
        public bool HasCard { get; set; }

        [JsonProperty("cardTitle")]
        public string CardTitle { get; set; }

        [JsonProperty("displayDlg")]
        public string DisplayDlg { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("followUpPrompt")]
        public string FollowUpPrompt { get; set; }

        [JsonProperty("nodeName")]
        public string NodeName { get; set; }

    }
}
