using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IFileReader
    {

        Task<StoryTitle> GetTitleContentsAsync(TitleVersion titleVersion);


    }
}
