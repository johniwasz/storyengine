using System.IO;

namespace Whetstone.StoryEngine.Models
{
    public class SimpleMediaResponse
    {

        public SimpleMediaResponse(MemoryStream mediaStream, string contentType)
        {
            MediaStream = mediaStream;
            ContentType = contentType;
        }

        public MemoryStream MediaStream { get; private set; }

        public string ContentType { get; private set; }
    }
}
