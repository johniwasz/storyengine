using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace Whetstone.StoryEngine.Models.Integration
{


    public enum DataRetrievalType
    {
        TableSearch = 1,
        ExternalRequest =2

    }


    [DataContract]
    [Table("NodeDataRetrieval")]
    [XmlInclude(typeof(TableFunctionSearchAction))]
    [XmlInclude(typeof(ExternalFunctionAction))]
    [MessagePack.Union(0, typeof(TableFunctionSearchAction))]
    [MessagePack.Union(1, typeof(ExternalFunctionAction))]
    public abstract class DataRetrievalAction
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        public long? Id { get; set; }


       
        public abstract DataRetrievalType DataRetrievalType { get; set; }



        public abstract bool? CacheResult { get; set; }

    }
}
