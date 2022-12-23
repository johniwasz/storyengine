using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story.Cards
{

    [JsonObject]
    [DataContract]
    public class LinkButton : CardButton
    {
        public LinkButton()
        {
            ButtonType = CardButtonType.Link;

        }


        [NotMapped]
        [JsonProperty("buttonType")]
        [MessagePack.Key(0)]
        [YamlIgnore]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        public sealed override CardButtonType ButtonType { get; set; }

        /// <summary>
        /// Url to send the user to on the button
        /// </summary>
        [YamlMember(Alias = "url")]
        [MessagePack.Key(1)]
        [DataMember()]
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }

        /// <summary>
        /// Link text to display on the button
        /// </summary>
        [YamlMember(Alias = "linkText")]
        [DataMember()]
        [MessagePack.Key(2)]
        [JsonProperty("linkText", NullValueHandling = NullValueHandling.Ignore)]
        public string LinkText { get; set; }
    }
}
