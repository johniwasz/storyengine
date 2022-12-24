
using System.IO;
using Whetstone.StoryEngine.Data.MimeTypes;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.FileStorage
{
    public abstract class FileLinkStore
    {


        protected virtual string GetMimeByFileName(string fileName)
        {
            string fileExt = Path.GetExtension(fileName);

            return MimeTypeMap.GetMimeType(fileExt);


        }

        protected virtual string GetMimeCategory(string mimeType)
        {
            string mediaType = "unknown";
            if (!string.IsNullOrWhiteSpace(mimeType) && mimeType.Contains(@"/"))
            {
                mediaType = mimeType.Split('/')[0];
            }

            return mediaType;
        }

        protected string GetTitlePath(TitleVersion titleVer)
        {

            string titlePath;
            if (string.IsNullOrWhiteSpace(titleVer.Version))
                titlePath = string.Concat(@"stories/", titleVer.ShortName, @"/", titleVer.ShortName, ".yaml");
            else
                titlePath = string.Concat(@"stories/", titleVer.ShortName, @"/", titleVer.Version, @"/", titleVer.ShortName, ".yaml");

            return titlePath;
        }

        protected string GetDebugTitlePath(TitleVersion titleVer)
        {

            string titlePath;
            if (string.IsNullOrWhiteSpace(titleVer.Version))
                titlePath = string.Concat(titleVer.ShortName, @"\", titleVer.ShortName, ".yaml");
            else
                titlePath = string.Concat(titleVer.ShortName, @"\", titleVer.Version, @"\", titleVer.ShortName, ".yaml");

            return titlePath;
        }


        protected virtual string GetInternalPath(TitleVersion titleVer, string fileName, bool includeMediaPath = true)
        {

            string mimeType = GetMimeByFileName(fileName);

            return GetInternalPath(titleVer, fileName, mimeType, includeMediaPath);
        }

        protected string GetInternalPath(TitleVersion titleVer, string fileName, string mediaType, bool includeMediaPath = true)
        {
            string internalPath;
            string mediaPath = GetMimeCategory(mediaType);

            if (includeMediaPath)
            {
                // If no title is provided, then assume the file is a global value.
                if (string.IsNullOrWhiteSpace(titleVer.ShortName))
                    internalPath = string.Concat(@"global/", mediaPath, @"/", fileName);
                else
                {
                    if (string.IsNullOrWhiteSpace(titleVer.Version))
                        internalPath = string.Concat(@"stories/", titleVer.ShortName, @"/", mediaPath, @"/", fileName);
                    else
                        internalPath = string.Concat(@"stories/", titleVer.ShortName, @"/", titleVer.Version, @"/", mediaPath, @"/", fileName);

                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(titleVer.ShortName))
                    internalPath = string.Concat(@"global/", fileName);
                else
                {

                    if (string.IsNullOrWhiteSpace(titleVer.Version))
                        internalPath = string.Concat(@"stories/", titleVer.ShortName, @"/", fileName);
                    else
                        internalPath = string.Concat(@"stories/", titleVer.ShortName, @"/", titleVer.Version, @"/", fileName);
                }
            }
            return internalPath;
        }

    }
}
