using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{

    public enum StoryType
    {
        AppExperience =0,
        SingleRequest =1

    }


    [DataContract]
    public class StoryTitle 
    {

        /// <summary>
        /// Collection id of the audio skill.
        /// </summary>
        //  public ObjectId Id { get; set; }
        [Key]
        [DataMember]
        [YamlMember(Alias = "id", Order = 0, ApplyNamingConventions =false)]
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>
        /// Database identifier of the title
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "dataTitleId", Order = 1, ApplyNamingConventions = false)]
        [JsonProperty(PropertyName = "dataTitleId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? DataTitleId { get; set; }


        /// <summary>
        /// Database identifier of the title version
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "dataVersionTitleId", Order = 2, ApplyNamingConventions = false)]
        [JsonProperty(PropertyName = "dataVersionTitleId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? DataVersionTitleId { get; set; }


        /// <summary>
        /// Public title of the audio skill.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Alias = "title", Order = 3)]
        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [DataMember]
        [YamlMember(Alias = "version", Order = 4)]
        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }


        [DataMember(EmitDefaultValue =false)]        
        [YamlMember(Alias = "storyType", Order = 5)]
        [JsonProperty(PropertyName = "storyType", NullValueHandling = NullValueHandling.Ignore)]
        public StoryType StoryType { get; set; }


        /// <summary>
        /// Date the audio skill was pubished. If it has not yet been published, the date is null.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "publishDate", Order = 6)]
        [JsonProperty(PropertyName = "publishDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? PublishDate { get; set; }


        /// <summary>
        /// If set to true, selected intents and slot values are not logged. This will impact troubleshooting Defaults to false.
        /// </summary>
        [DataMember]
        [YamlMember(Alias = "enablePrivacyLogging", Order = 7)]
        [JsonProperty(PropertyName = "enablePrivacyLogging", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EnablePrivacyLogging { get; set; }


        /// <summary>
        /// Summary of the skill.
        /// </summary>   
        [DataMember]
        [YamlMember(Alias ="description", Order = 8)]
        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [DataMember]
        [YamlMember(Alias = "invocationNames", Order = 9)]
        [JsonProperty(PropertyName = "invocationNames", NullValueHandling = NullValueHandling.Ignore)]
        public List<LocalizedPlainText> InvocationNames { get; set; }


        [DataMember]
        [YamlMember(Alias = "phoneInfo", Order = 10)]
        [JsonProperty(PropertyName = "phoneInfo", NullValueHandling = NullValueHandling.Ignore)]
        public StoryPhoneInfo PhoneInfo { get; set; }

        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "startNodeName", Order = 11)]
        [JsonProperty(PropertyName = "startNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string StartNodeName { get; set; }



        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "newUserNodeName", Order = 12)]
        [JsonProperty(PropertyName = "newUserNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string NewUserNodeName { get; set; }

        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "returningUserNodeName", Order = 13)]
        [JsonProperty(PropertyName = "returningUserNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string ReturningUserNodeName { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "resumeNodeName", Order =14)]
        [JsonProperty(PropertyName = "resumeNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string ResumeNodeName { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "helpNodeName", Order = 15)]
        [JsonProperty(PropertyName = "helpNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string HelpNodeName { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "stopNodeName", Order = 16)]
        [JsonProperty(PropertyName = "stopNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string StopNodeName { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "endOfGameNodeName", Order = 17)]
        [JsonProperty(PropertyName = "endOfGameNodeName", NullValueHandling = NullValueHandling.Ignore)]
        public string EndOfGameNodeName { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "appendEndTextOnExit", Order = 18)]
        [JsonProperty(PropertyName = "appendEndTextOnExit", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AppendEndTextOnExit { get; set; }


        /// <summary>
        /// This is for serialization and deserializaton.
        /// </summary>
        /// <remarks>Do not store to the database in this format.</remarks>      
        [DataMember]
        [InverseProperty("Title")]
        [YamlMember(Alias ="nodes", Order = 19)]
        [JsonProperty(PropertyName = "nodes", NullValueHandling = NullValueHandling.Ignore)]
        public List<StoryNode> Nodes { get; set; }

        /// <summary>
        /// For serialization
        /// </summary>        
        [DataMember]
        [YamlMember(Alias ="intents", Order = 20)]
        [JsonProperty(PropertyName = "intents", NullValueHandling = NullValueHandling.Ignore)]
        public List<Intent> Intents { get; set; }


        [DataMember]
        [NotMapped]
        [YamlMember(Alias = "conditions", Order = 21)]
        [JsonProperty(PropertyName = "conditions", NullValueHandling = NullValueHandling.Ignore)]
        public List<StoryConditionBase> Conditions { get; set; }

        


        [NotMapped]
        [DataMember]     
        //[InverseProperty("BadResponseTitle")]
        [YamlMember(Alias = "badIntentResponses", Order = 22)]
        [JsonProperty(PropertyName = "badIntentResponses", NullValueHandling = NullValueHandling.Ignore)]
        public List<StoryNode> BadIntentResponses { get; set; }


        [NotMapped]
        [DataMember]
        //[InverseProperty("BadResponseTitle")]
        [YamlMember(Alias = "errorResponses", Order = 23)]
        [JsonProperty(PropertyName = "errorResponses", NullValueHandling = NullValueHandling.Ignore)]
        public List<StoryNode> ErrorResponses { get; set; }




        [DataMember]
        [YamlMember(Alias = "slotTypes",Order = 24)]
        [JsonProperty(PropertyName = "slotTypes", NullValueHandling = NullValueHandling.Ignore)]
        public List<SlotType> Slots { get; set; }
        


    }


}
