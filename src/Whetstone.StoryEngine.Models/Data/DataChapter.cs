using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Story;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Whetstone.StoryEngine.Models.Data
{

    [Table("Chapters")]
    public class DataChapter
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("Id", Order = 0)]
        public long? Id { get; set; }

        [Required]
        [DataMember]
        public int Sequence { get; set; }

        

        [Column("Names", TypeName = "jsonb")]
        private string NamesJson { get; set; }


        [NotMapped]
        [DataMember]
        public List<LocalizedPlainText> Names
        {
            get
            {
                return JsonConvert.DeserializeObject<List<LocalizedPlainText>>(NamesJson);

            }
            set
            {
               NamesJson = JsonConvert.SerializeObject(value);
            }
        }


        /// <summary>
        /// This is for serialization and deserializaton.
        /// </summary>
        /// <remarks>Do not store to the database in this format.</remarks>      
        [DataMember]  
        [ForeignKey("ChapterId")]
        public List<DataNode> Nodes { get; set; }

    }
}
