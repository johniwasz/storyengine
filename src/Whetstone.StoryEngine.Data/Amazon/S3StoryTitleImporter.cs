using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Serialization;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.Amazon
{
    public class S3StoryTitleImporter : IStoryTitleImporter
    {

        private readonly IFileRepository _fileRep;

        public S3StoryTitleImporter(IFileRepository fileRep)
        {
            _fileRep = fileRep;
        }


        public async Task ImportFromZipAsync(byte[] importZip)
        {
            if (importZip == null || importZip.Length == 0)
                throw new ArgumentException("importZip is null or empty");



            using (MemoryStream importBytes = new MemoryStream(importZip))
            {
                using (ZipArchive archive = new ZipArchive(importBytes, ZipArchiveMode.Read))
                {
                    ZipArchiveEntry titleEntry = archive.Entries.FirstOrDefault(x => x.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));
                    TitleVersion titleVersion = new TitleVersion();

                    if (titleEntry != null)
                    {

                        string rawText = titleEntry.ToText();
                        var deser = YamlSerializationBuilder.GetYamlDeserializer();

                        StoryTitle title = deser.Deserialize<StoryTitle>(rawText);
                        titleVersion.ShortName = title.Id;
                        titleVersion.Version = title.Version;
                        await _fileRep.StoreTitleAsync(title);
                    }

                    var mediaEntries = archive.Entries.Where(x => !x.Name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase));

                    foreach (ZipArchiveEntry entry in mediaEntries)
                    {
                        byte[] bytes = entry.ToByteArray();
                        // It's a media file.
                        await _fileRep.StoreFileAsync(titleVersion, entry.Name, bytes);
                    }
                }
            }
        }
    }
}
