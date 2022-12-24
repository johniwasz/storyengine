using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Models.Data
{



    [JsonObject(Title = "Version")]
    [DataContract]
    [DebuggerDisplay("TitleId = {TitleId}, Version = {Version}")]
    [Table("titleversions")]
    public class DataTitleVersion
    {

        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        public Guid? Id { get; set; }


        [DataMember]
        [Column("titleid", Order = 1)]
        public Guid TitleId { get; set; }


        [Required]
        [DataMember]
        [Column("version", Order = 2)]
        public string Version { get; set; }

        [DataMember]
        [Column("description", Order = 3)]
        public string Description { get; set; }


        [IgnoreDataMember] public DataTitle Title { get; set; }


        [Column("isdeleted", Order = 4)]
        [DataMember]
        public bool IsDeleted { get; set; }


        [Column("deletedate", Order = 5)]
        [DataMember]
        public DateTime? DeleteDate { get; set; }


        /// <summary>
        /// Forces the system to log the full inbound and outbound client messages.
        /// </summary>
        /// <remarks>
        /// This overrides the default environment wide settings. For example, if the lambda environment settings are configured not to log all client messages and
        /// this value is set to true, then all client message are logged anyway.
        /// </remarks>
        [Column("logfullclientmessages", Order = 6)]
        [DataMember]
        public bool LogFullClientMessages { get; set; }

        [ForeignKey("TitleVersionId")]
        public List<DataTwitterApplication> TwitterApplications { get; set; }

        [ForeignKey("VersionId")]
        public List<DataTitleVersionDeployment> VersionDeployments { get; set; }


        [ForeignKey("TitleVersionId")]
        public List<UserPhoneConsent> PhoneConsentRecords { get; set; }


        public static explicit operator DataTitleVersion(TitleVersion titleVer)
        {
            DataTitleVersion datatitleVer = null;

            if (titleVer != null)
            {
                datatitleVer = new DataTitleVersion();

                if (titleVer.TitleId.HasValue)
                    datatitleVer.Id = titleVer.TitleId.Value;

                datatitleVer.IsDeleted = false;

                datatitleVer.Version = titleVer.Version;

                if (!string.IsNullOrWhiteSpace(titleVer.ShortName))
                {
                    datatitleVer.Title = new DataTitle();
                    datatitleVer.Title.ShortName = titleVer.ShortName;
                    if (titleVer.TitleId.HasValue)
                        datatitleVer.Id = titleVer.TitleId;

                    if (titleVer.VersionId.HasValue)
                    {
                        datatitleVer.Title.Versions = new List<DataTitleVersion>();
                        DataTitleVersion dataVer = new DataTitleVersion
                        {
                            Id = titleVer.VersionId.Value,
                            Version = titleVer.Version,
                            LogFullClientMessages = titleVer.LogFullClientMessages
                        };
                        if (titleVer.DeploymentId.HasValue)
                        {
                            dataVer.VersionDeployments = new List<DataTitleVersionDeployment>();
                            DataTitleVersionDeployment titleDeployment = new DataTitleVersionDeployment();
                            titleDeployment.Id = titleVer.DeploymentId.Value;
                            titleDeployment.Alias = titleVer.Alias;
                            dataVer.VersionDeployments.Add(titleDeployment);
                        }

                        var versionList = datatitleVer.Title.Versions.ToList();

                        versionList.Add(dataVer);

                    }

                }


            }

            return datatitleVer;
        }
    }
}
