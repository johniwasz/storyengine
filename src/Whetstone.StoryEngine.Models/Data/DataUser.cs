using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace Whetstone.StoryEngine.Models.Data
{

    [JsonObject(Title = "User")]
    [DataContract]
    [DebuggerDisplay("UserId = {Id}, CognitoSub = {CognitoSub}")]
    [Table("users")]
    public class DataUser
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        public Guid? Id { get; set; }


        [DataMember]
        [Column("cognito_sub", Order = 1)]
        public string CognitoSub { get; set; }


        public IEnumerable<DataUserGroupXRef> GroupXRefs { get; set; }
    }


}
