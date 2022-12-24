using System.IO;
using System.IO.Compression;

namespace Whetstone.StoryEngine.Data
{
    internal static class ZipExtensions
    {
        internal static byte[] ToByteArray(this ZipArchiveEntry entry)
        {
            byte[] bytes = null;
            using (var stream = entry.Open())
            {

                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                }
            }
            return bytes;
        }


        internal static string ToText(this ZipArchiveEntry entry)
        {

            string rawText;
            using (var stream = entry.Open())
            {
                using (var reader = new StreamReader(stream))
                {
                    rawText = reader.ReadToEnd();
                }
            }

            return rawText;


        }

    }
}
