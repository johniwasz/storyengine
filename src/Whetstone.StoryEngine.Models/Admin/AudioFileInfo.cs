﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class AudioFileInfo
    {

        [JsonProperty(PropertyName = "fileName")]
        public string FileName { get; set; }

        [JsonProperty(PropertyName = "size")]
        public long? Size { get; set; }


        [JsonProperty(PropertyName = "lastModified")]
        public DateTime? LastModified { get; set; }


    }
}
