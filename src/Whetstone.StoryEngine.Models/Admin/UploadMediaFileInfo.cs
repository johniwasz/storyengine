using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public enum MediaFileType
    {
        None,
        Image,
        Audio
    }

    public class UploadMediaFileInfo
    {
        [JsonProperty(PropertyName = "projectId")]
        public Guid ProjectId;

        [JsonProperty(PropertyName = "versionId")]
        public Guid VersionId;

        [JsonProperty(PropertyName = "fileType")]
        public MediaFileType FileType;

        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "originalFileName")]
        public string OriginalFileName { get; set; }
    }
}
