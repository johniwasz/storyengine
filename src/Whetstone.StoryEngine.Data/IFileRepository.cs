using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{


    public interface IFileRepository : IFileReader
    {
        Task StoreFileAsync(TitleVersion titleVersion, string fileName, byte[] contents);

        Task<byte[]> GetFileContentAsync(TitleVersion titleVersion, string fileName);

        Task<T> GetJsonFileAsync<T>(TitleVersion titleVersion, string fileName, JsonSerializerSettings settings);

        Task<T> GetJsonFileAsync<T>(TitleVersion titleVersion, string fileName);

        Task<bool> DoesFileExistAsync(TitleVersion titleVersion, string fileName);

        Task StoreTitleAsync(StoryTitle title);


        Task<List<string>> GetAudioFileListAsync(TitleVersion titleVersion);


        Task<IEnumerable<AudioFileInfo>> GetAudioFileListAsync(Guid projectId, Guid versionId);


        Task CopyMediaFilesAsync(string titleId, string sourceVersion, string destVersion);


        Task PurgeTitleAsync(TitleVersion titleVer);


        Task DeleteTitleAsync(TitleVersion titleVer);


        Task<string> GetTextContentAsync(string fileName);

        Task SetTextContentAsync(string fileName, string contents, string mimeType);

        Task<FileContent> GetFileContentAsync(Guid projectId, Guid versionId, string fileName);


        Task<FileContentStream> GetFileContentStreamAsync(Guid projectId, Guid versionId, string fileName);

        Task<AudioFileInfo> StoreAudioFileAsync(Guid projectId, Guid versionId, string fileName, Stream stm);

        Task<AudioFileInfo> GetAudioFileInfoAsync(Guid projectId, Guid versionId, string fileName);

        Task DeleteAudioFileAsync(Guid projectId, Guid versionId, string fileName);

        Task<UploadMediaFileInfo> UploadAudioFileAsync(Guid projectId, Guid versionId, string fileName, Stream stm);

        Task<AudioFileInfo> CommitUploadedAudioFileAsync(UploadMediaFileInfo uploadFileInfo);

        // Direct helper function for reading a file from our S3 bucket
        Task<Stream> GetFileContentsAsync(string filePath);

        // Direct helper function for writing a file to our S3 bucket
        Task UploadFileContentsAsync(string filePath, string mimeType, Stream stm);
    }
}
