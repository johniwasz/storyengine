using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Conditions;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public class DataTitleRepository : EntityBaseRepository, ITitleRepository
    {
        private readonly ITitleReader _titleReader;
        private readonly IFileRepository _fileRep;

        public DataTitleRepository(IUserContextRetriever userContextRetriever,
                 ITitleCacheRepository titleCacheRep, ITitleReader titleReader, IFileRepository fileRep) : base(userContextRetriever, titleCacheRep)
        {
            _fileRep = fileRep;
            _titleReader = titleReader;
        }



        public async Task UpdateTitleAsync(StoryTitle entity)
        {
            await _fileRep.StoreTitleAsync(entity);
        }

        public async Task ClearTitleAsync(TitleVersion titleVersion)
        {
            await _titleReader.ClearTitleAsync(titleVersion);
        }

        /// <summary>
        /// Adds a new title entry to the titles table. 
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<StoryTitle> CreateOrUpdateTitleAsync(StoryTitle entity)
        {


            if (entity == null)
                throw new ArgumentNullException($"{nameof(entity)}");


            DataTitle retDataTitle = await CreateOrUpdateTitleAsync( entity.DataTitleId, entity.Id, entity.Title, entity.Description);
            // Assign the data title id to the stored story title

            StoryTitle retTitle = entity;
            retTitle.DataTitleId = retDataTitle.Id;


            return retTitle;
        }


        public Task DeleteTitleAsync(StoryTitle entity)

        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!entity.DataTitleId.HasValue)
                throw new ArgumentNullException(nameof(entity), "DataTitleId cannot be null");

            // TODO add a flag to mark the title as deleted. This is really a purge operation.

            //await DeleteTitleAsync(entity.DataTitleId);
            return Task.FromException(new NotImplementedException());
        }

        public async Task<StoryNode> GetBadIntentNodeAsync(TitleVersion titleVersion, int badIntentCount)
        {
            return await _titleReader.GetBadIntentNodeAsync(titleVersion, badIntentCount);
        }

        public async Task<StoryTitle> GetByIdAsync( TitleVersion titleVersion)
        {
            return await _titleReader.GetByIdAsync( titleVersion);
        }

        public async Task<Intent> GetIntentByNameAsync(TitleVersion titleVersion, string intentName)
        {
            return await _titleReader.GetIntentByNameAsync( titleVersion, intentName);
        }

        public async Task<List<Intent>> GetIntentsAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetIntentsAsync( titleVersion);
        }

        public async Task<StoryNode> GetNodeByNameAsync(TitleVersion titleVersion, string storyNodeName)
        {
            return await _titleReader.GetNodeByNameAsync( titleVersion, storyNodeName);
        }

        public async Task<ICollection<StoryNode>> GetNodesByTitleAsync(TitleVersion titleVersion)
        {
            return await _titleReader.GetNodesByTitleAsync( titleVersion);
        }

        public async Task<StoryPhoneInfo> GetPhoneInfoAsync( TitleVersion titleVersion)
        {
            return await _titleReader.GetPhoneInfoAsync( titleVersion);
        }

        public async Task<List<SlotType>> GetSlotTypes(TitleVersion titleVersion)
        {
            return await _titleReader.GetSlotTypes( titleVersion);
        }

        public async Task<string> GetStartNodeNameAsync( TitleVersion titleVersion, bool isNew)
        {
            return await _titleReader.GetStartNodeNameAsync( titleVersion, isNew);
        }

        public async Task<StoryConditionBase> GetStoryConditionAsync( TitleVersion titleVersion, string conditionName)
        {
            return await _titleReader.GetStoryConditionAsync( titleVersion, conditionName);
        }

        public async Task<List<StoryConditionBase>> GetStoryConditionsAsync( TitleVersion titleVersion)
        {
            return await _titleReader.GetStoryConditionsAsync(titleVersion);
        }

        public async Task<StoryType> GetStoryTypeAsync( TitleVersion titleVersion)
        {
            return await _titleReader.GetStoryTypeAsync(titleVersion);
        }

        public async Task<StoryNode> GetErrorNodeAsync( TitleVersion titleVersion)
        {
            return await _titleReader.GetErrorNodeAsync( titleVersion);
        }
        public async Task<bool> IsPrivacyLoggingEnabledAsync(TitleVersion titleVersion)
        {


            string titleId = titleVersion.ShortName;

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            var storyTitle = await GetByIdAsync(titleVersion);
            return storyTitle.EnablePrivacyLogging.GetValueOrDefault(false);

        }

        public async Task<List<TitleRoot>> GetAllTitleDeploymentsAsync()
        {

            List<TitleRoot> allTitles = new List<TitleRoot>();


            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {

                var titleFound = userContext.Titles
                    .Include(a => a.Versions)
                    .ThenInclude(tv => tv.VersionDeployments);

                await titleFound.LoadAsync();


                foreach (var title in titleFound)
                {

                    TitleRoot root = new TitleRoot
                    {
                        Id = title.Id.Value,
                        ShortName = title.ShortName,
                        Description = title.Description
                    };


                    if ((title.Versions?.Any()).GetValueOrDefault(false))
                    {


                        root.Versions = new List<TitleVersionAdmin>();
                        foreach (var ver in title.Versions)
                        {
                            if (!ver.IsDeleted)
                            {
                                TitleVersionAdmin version = new TitleVersionAdmin
                                {
                                    Id = ver.Id.Value,
                                    Version = ver.Version,
                                    Description = ver.Description,
                                    LogFullClientMessages = ver.LogFullClientMessages
                                };

                                if ((ver.VersionDeployments?.Any()).GetValueOrDefault(false))
                                {
                                    version.Deployments = new List<TitleVersionDeploymentBasic>();
                                    foreach (var verDep in ver.VersionDeployments)
                                    {
                                        if (!verDep.IsDeleted)
                                        {
                                            TitleVersionDeploymentBasic titleVerDep = new TitleVersionDeploymentBasic
                                            {
                                                Id = verDep.Id.Value,
                                                PublishDate = verDep.PublishDate,
                                                ClientId = verDep.ClientIdentifier,
                                                ClientType = verDep.Client,
                                                Alias = verDep.Alias
                                            };
                                            version.Deployments.Add(titleVerDep);
                                        }
                                    }

                                    if (version.Deployments.Count == 0)
                                        version.Deployments = null;
                                }

                                root.Versions.Add(version);
                            }
                        }

                        if (root.Versions.Count == 0)
                            root.Versions = null;

                    }

                    allTitles.Add(root);
                }
            }


            return allTitles;
        }
    }
}
