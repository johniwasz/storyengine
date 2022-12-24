

using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story.Ssml;

namespace Whetstone.StoryEngine.Models.Story.Text
{
    public enum TextFragmentType
    {
        Simple = 1,
        Conditional = 2,
        Audio = 3
    }

    [DataContract]
    [Table("textfragments")]
    [JsonConverter(typeof(TextFragmentConverter))]
    [XmlInclude(typeof(SimpleTextFragment))]
    [XmlInclude(typeof(ConditionalTextFragment))]
    [XmlInclude(typeof(AudioTextFragment))]
    [MessagePack.Union(0, typeof(SimpleTextFragment))]
    [MessagePack.Union(1, typeof(ConditionalTextFragment))]
    [MessagePack.Union(2, typeof(AudioTextFragment))]
    public abstract class TextFragmentBase
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [MessagePack.Key(0)]
        [Key()]
        public long? Id { get; set; }

        [NotMapped] public abstract TextFragmentType FragmentType { get; set; }

        public abstract SpeechFragment ToSpeechFragment();
    }


}
