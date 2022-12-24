using System.IO;

namespace Whetstone.StoryEngine.Models.Admin
{
    public class FileContentStream
    {



        public string FileName { get; set; }


        public Stream Content { get; set; }


        public string MimeType { get; set; }
    }
}
