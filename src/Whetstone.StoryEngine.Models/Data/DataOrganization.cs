﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Whetstone.StoryEngine.Models.Data
{
    [JsonObject(Title = "Organization")]
    [DataContract]
    [DebuggerDisplay("Id = {Id}, Name = {Name}")]
    [Table("organizations")]
    public class DataOrganization
    {
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [System.ComponentModel.DataAnnotations.Key]
        [Column("id", Order = 0)]
        public Guid? Id { get; set; }

        [Required]
        [DataMember]
        [Column("name", Order = 1)]
        public string Name { get; set; }

        [Required]
        [DataMember]
        [Column("description", Order = 2)]
        public string Description { get; set; }

        [Required]
        [DataMember]
        [Column("subscriptionlevel_id", Order = 3)]
        public Guid SubscriptionLevelId { get; set; }


        [DataMember]
        [Column("isenabled", Order = 4)]
        public bool IsEnabled { get; set; }


        [ForeignKey("OrganizationId")]
        public List<DataGroup> Groups { get; set; }

        [ForeignKey("OrganizationId")]
        public List<DataTwitterApplication> TwitterApplications { get; set; }
    }


}
