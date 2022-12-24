using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Data
{
    [DebuggerDisplay("Intent Name = {Name}")]
    [Table("Intents")]
    public class DataIntent : IStoryDataItem
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("Id", Order = 0)]
        public long? Id { get; set; }

        [Required]
        public Guid UniqueId { get; set; }


        public DataIntent()
        {
        }

        public DataIntent(string localizedIntents)
        {

            this.LocalizedIntentsJson = localizedIntents;
        }

        /// <summary>
        /// Plain text name of the intent. It must be unique.
        /// </summary>
        [Required]
        [DataMember]
        public string Name { get; set; }


        [Column("VersionId")]
        [DataMember]
        public long? VersionId { get; set; }




        [Column("LocalizedIntents", TypeName = "jsonb")]
        private string LocalizedIntentsJson { get; set; }

        [JsonProperty(IsReference = false)]
        [NotMapped]
        [DataMember]
        public List<LocalizedIntent> LocalizedIntents
        {
            get
            {
                return JsonConvert.DeserializeObject<List<LocalizedIntent>>(LocalizedIntentsJson);
            }
            set
            {
                LocalizedIntentsJson = JsonConvert.SerializeObject(value);
            }

        }

        [ForeignKey("IntentId")]
        [DataMember]
        public List<DataIntentSlotMapping> SlotTypeMappings { get; set; }

    }
}
