using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface ITitleRepository : ITitleReader
    {
        Task<StoryTitle> CreateOrUpdateTitleAsync( StoryTitle entity);

        Task DeleteTitleAsync(StoryTitle entity);

        Task<List<TitleRoot>> GetAllTitleDeploymentsAsync();
    }
}
