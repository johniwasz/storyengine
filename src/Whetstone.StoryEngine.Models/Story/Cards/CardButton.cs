using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Xml.Serialization;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Cards
{
    public enum CardButtonType
    {
        Link = 1
    }

    [XmlInclude(typeof(LinkButton))]
    [JsonConverter(typeof(CardButtonConverter))]
    [MessagePack.Union(0, typeof(LinkButton))]
    public abstract class CardButton 
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("buttonType", Order = 0, NullValueHandling = NullValueHandling.Ignore)]
        public abstract CardButtonType ButtonType { get; set; }
    }
}