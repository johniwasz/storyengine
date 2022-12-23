using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Models.Configuration
{





    public class EncryptionConfig
    {
        public string Key { get; set; }

        public string KeyVector { get; set; }


        public int? LinkTimeout { get; set; }

        public string MediaRoute { get; set; }
    }

}
