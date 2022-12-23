using Whetstone.StoryEngine.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Repository
{
    public interface IMediaStreamer
    {

        Task<SimpleMediaResponse> GetFileStreamAsync(string environment, TitleVersion titleVer, string fileName, bool isFileEncrypted= true);

    }
}
