using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class TitleVersionConfiguration
    {

        [JsonProperty(PropertyName = "titleId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? TitleId { get; set; }


        [JsonProperty(PropertyName = "versionId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? VersionId { get; set; }

        [JsonProperty(PropertyName = "titleName", NullValueHandling = NullValueHandling.Ignore)]
        public string TitleName { get; set; }

        [JsonProperty(PropertyName = "version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }


        [JsonProperty(PropertyName = "isDeleted", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDeleted { get; set; }

        [JsonProperty(PropertyName = "description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }


        /// <summary>
        /// This enables and disables detailed message logging.
        /// </summary>
        [JsonProperty(PropertyName = "logFullClientMessages", NullValueHandling = NullValueHandling.Ignore)]
        public bool LogFullClientMessages { get; set; }


        [JsonProperty(PropertyName = "deleteDate", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? DeletedDate { get; set; }
    }
}
