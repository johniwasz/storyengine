using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{
    public class DebugConfig
    {

        [JsonIgnore]
        [YamlIgnore]
        public LocalFileConfig LocalFileConfig { get; set; }
    }
}
