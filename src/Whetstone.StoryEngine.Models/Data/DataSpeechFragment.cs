using MessagePack;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{


    [JsonConverter(typeof(DataSpeechFragmentConverter))]
    [DataContract]
    [Table("SpeechFragments")]
    [XmlInclude(typeof(DataAudioFile))]
    [XmlInclude(typeof(DataDirectAudioFile))]
    [XmlInclude(typeof(DataSpeechText))]
    [XmlInclude(typeof(DataConditionalFragment))]
    [XmlInclude(typeof(DataSsmlSpeechFragment))]
    [XmlInclude(typeof(DataSpeechBreakFragment))]
    [MessagePack.Union(0, typeof(DataAudioFile))]
    [MessagePack.Union(1, typeof(DataDirectAudioFile))]
    [MessagePack.Union(2, typeof(DataSpeechText))]
    [MessagePack.Union(3, typeof(DataConditionalFragment))]
    [MessagePack.Union(4, typeof(DataSsmlSpeechFragment))]
    [MessagePack.Union(5, typeof(DataSpeechBreakFragment))]
    public abstract class DataSpeechFragment
    {

        [MessagePack.Key(0)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key()]
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [IgnoreDataMember]
        [DataMember]
        public int Sequence { get; set; }

        [IgnoreDataMember]
        [DataMember]
        public string Comment { get; set; }

        [IgnoreDataMember]
        [JsonIgnore()]
        public long? TrueResultParentId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore()]
        public long? FalseResultParentId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore()]
        public long? VersionId { get; set; }

        [IgnoreDataMember]
        [JsonIgnore()]
        [ForeignKey("VersionId")]
        public DataTitleVersion StoryVersion { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public abstract SpeechFragmentType FragmentType { get; set; }

        public DataSpeechFragment()
        {
        }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataAudioFile : DataSpeechFragment
    {

        public DataAudioFile()
        {
            this.FragmentType = SpeechFragmentType.AudioFile;
        }

        public DataAudioFile(string fileName)
        {
            this.FragmentType = SpeechFragmentType.AudioFile;
            this.FileName = fileName;

        }

        [JsonProperty("fileName", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(1)]
        //public Uri AudioUrl
        public string FileName
        {
            get; set;
        }

        [NotMapped]
        [DataMember]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("fragmentType", Order = 0)]
        [Key(2)]
        public sealed override SpeechFragmentType FragmentType { get; set; }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataSpeechText : DataSpeechFragment
    {

        private string _speechText;

        public DataSpeechText()
        {
            FragmentType = SpeechFragmentType.PlainText;
        }

        public DataSpeechText(string speechText)
        {

            FragmentType = SpeechFragmentType.PlainText;
            _speechText = speechText;

        }

        [JsonProperty("text", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(1)]
        public string Text { get { return _speechText; } set { _speechText = value; } }

        [Column("TextVoice")]
        [JsonProperty("voice", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(2)]
        public string Voice { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType", Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(3)]
        public sealed override SpeechFragmentType FragmentType { get; set; }

        [Column("VoiceFileId")]
        [JsonProperty("voiceFileId", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(4)]
        public Guid? VoiceFileId { get; set; }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataSpeechBreakFragment : DataSpeechFragment
    {

        public DataSpeechBreakFragment()
        {

            FragmentType = SpeechFragmentType.Break;
        }

        [JsonProperty("duration", Order = 1)]
        [DataMember]
        [Key(1)]
        public int Duration { get; set; }


        [NotMapped]
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("fragmentType", Order = 0)]
        [DataMember]
        public override SpeechFragmentType FragmentType { get; set; }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataDirectAudioFile : DataSpeechFragment
    {


        public DataDirectAudioFile()
        {

            FragmentType = SpeechFragmentType.DirectAudioFile;
        }


        public DataDirectAudioFile(string audioUri)
        {
            this.AudioUrl = audioUri;
            FragmentType = SpeechFragmentType.DirectAudioFile;

        }

        [JsonProperty("audioUrl", Order = 1)]
        [DataMember]
        [Key(1)]
        //public Uri AudioUrl
        public string AudioUrl { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType", Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(2)]
        public sealed override SpeechFragmentType FragmentType { get; set; }
    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataConditionalFragment : DataSpeechFragment
    {

        public DataConditionalFragment()
        {
            FragmentType = SpeechFragmentType.ConditionalFragment;

        }

        [Key(1)]
        [DataMember]
        [JsonProperty("trueResultParentId", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [ForeignKey("TrueResultParentId")]
        public ICollection<DataSpeechFragment> TrueResultFragments { get; set; }


        ///// <summary>
        ///// If any one of the conditions is false, then use these speech fragments.
        ///// </summary>

        [Key(2)]
        [DataMember]
        [JsonProperty("falseResultParentId", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [ForeignKey("FalseResultParentId")]
        public ICollection<DataSpeechFragment> FalseResultFragments { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType", Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(3)]
        public sealed override SpeechFragmentType FragmentType { get; set; }

    }

    [JsonObject]
    [DataContract]
    [MessagePackObject]
    public class DataSsmlSpeechFragment : DataSpeechFragment
    {

        public DataSsmlSpeechFragment()
        {
            FragmentType = SpeechFragmentType.Ssml;
        }

        [JsonProperty("ssml", Order = 1, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(1)]
        public string Ssml { get; set; }


        [Column("SsmlVoice")]
        [JsonProperty("voice", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(2)]
        public string Voice { get; set; }

        [NotMapped]
        [JsonProperty("fragmentType", Order = 0)]
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember]
        [Key(3)]
        public sealed override SpeechFragmentType FragmentType { get; set; }


        [Column("VoiceFileId")]
        [JsonProperty("voiceFileId", Order = 3, NullValueHandling = NullValueHandling.Ignore)]
        [DataMember]
        [Key(4)]
        public Guid? VoiceFileId { get; set; }

    }
}
