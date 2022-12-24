using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{



    [JsonObject(Title = "TitleVersionDeployment")]
    [DataContract]
    [Table("titleversiondeployments")]
    public class DataTitleVersionDeployment
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        [DataMember]
        public Guid? Id { get; set; }

        [Column("versionid", Order = 1)]
        [DataMember]
        public Guid VersionId { get; set; }

        [Column("client", Order = 2)]
        [DataMember]
        public Client Client { get; set; }


        /// <summary>
        /// This is unique identifier used to associate the deployed title version with the client host. If the Client is Alexa, then the
        /// ClientIdentifier is the Alexa Skill Id.
        /// </summary>
        /// <remarks>For Alexa, this is the Alexa Skill Id. For Google, it's the Dialog Flow agent name. For a text-based SMS implementation, it's
        /// the inbound phone number of the SMS message.</remarks>
        [Column("clientidentifier", Order = 3)]
        public string ClientIdentifier { get; set; }


        [IgnoreDataMember]
        public DataTitleVersion Version { get; set; }

        /// <summary>
        /// Is used to map the alias associated with the client application with the deployed version.
        /// </summary>
        /// <remarks>
        /// The alias is provided by the Lambda function in the case of AWS Lambda functions. The alias is provided by a URL query string in the case of webhook implementations like DialogFlow/Google.
        /// </remarks>
        [Column("alias", Order = 4)]
        [DataMember]
        public string Alias { get; set; }



        [Column("publishdate", Order = 5)]
        [DataMember]
        public DateTime? PublishDate { get; set; }


        [Column("isdeleted", Order = 6)]
        [DataMember]
        public bool IsDeleted { get; set; }


        [Column("deletedate", Order = 7)]
        [DataMember]
        public DateTime? DeleteDate { get; set; }


    }
}
