using System;
using System.Collections.Generic;
using System.Text;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class FileContent
    {


        public string FileName { get; set; }


        public byte[] Content { get; set; }


        public string MimeType { get; set; }
    }
}
