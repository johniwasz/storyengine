using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Story
{


    [Serializable]
    [DataContract]
    [JsonObject(IsReference = false)]
    public class Coordinates
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("Id", Order = 0)]
        public long? Id { get; set; }


        public Coordinates()
        {

        }

        public Coordinates(int x, int y)
        {
            X = x;
            Y = y;

        }

        [DataMember]
        public int X { get; set; }


        [DataMember]
        public int Y { get; set; }
    }
}
