using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{

    [DebuggerDisplay("SlotType = {Name}")]
    [Table("SlotTypes")]
    public class DataSlotType : IStoryDataItem
    {

        public DataSlotType()
        {

        }


        public DataSlotType(string valueObject)
        {
            this.ValuesJson = valueObject;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }


        [DataMember]
        [Required]
        public Guid UniqueId { get; set; }

        [Required]
        [DataMember]
        public string Name { get; set; }



        [Column("ValuesJson", TypeName = "jsonb")]
        private string ValuesJson { get; set; }


        [NotMapped]
        [DataMember]
        public List<SlotValue> Values
        {
            get
            {
                return JsonConvert.DeserializeObject<List<SlotValue>>(ValuesJson);

            }
            set
            {
                ValuesJson = JsonConvert.SerializeObject(value);
            }

        }


        [ForeignKey("SlotTypeId")]
        [DataMember]
        public List<DataIntentSlotMapping> IntentSlotMappings { get; set; }


        [Required]
        [Column("VersionId")]
        public long? VersionId { get; set; }


    }


    //[DebuggerDisplay("SlotValue = {Value}")]
    //[Table("SlotValues")]
    //public class DataSlotValue : IStoryDataItem
    //{
    //    [Key]
    //    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    //    public long? Id { get; set; }




    //    [DataMember]
    //    [Required]
    //    public Guid UniqueId { get; set; }

    //    [Required]
    //    [DataMember]
    //    public string Value { get; set; }



    //    [Column("Synonyms")]
    //    [DataMember]
    //    public string[] Synonyms { get; set; }

    //    [Required]
    //    [Column("SlotTypeId")]
    //    public long? SlotTypeId { get; set; }

    //}
}
