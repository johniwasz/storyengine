using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data
{
    public interface IStoryVersionRepository
    {
        Task CreateOrUpdateVersionAsync(StoryTitle title);


        Task PublishVersionAsync(PublishVersionRequest publishRequest);

        /// <summary>
        /// Copies one version to another. Does not include prior version deployments. 
        /// </summary>
        /// <param name="titleId">Short name of the title</param>
        /// <param name="sourceVersion">Source version can be null or empty. If it is, the original root path is assumed</param>
        /// <param name="destVersion">Version destination.</param>
        /// <returns></returns>
        Task<StoryTitle> CloneVersionAsync(string titleId, string sourceVersion, string destVersion);

        Task<byte[]> ExportToZip(TitleVersion titleVer);


        Task DeleteVersionAsync(string titleName, string version);


        /// <summary>
        /// Remove all files and data rows related to the version. This is a hard delete.
        /// </summary>
        /// <param name="environment"></param>
        /// <param name="titleId"></param>
        /// <param name="version">Can be null or empty. Assumes root if it is.</param>
        /// <returns></returns>
        Task PurgeVersionAsync(string titleId, string version);


        Task<TitleVersionConfiguration> GetVersionConfigurationAsync(string titleId, string version);



        Task<TitleVersionConfiguration> GetVersionConfigurationAsync(string titleId, string version, bool includeDeleted);

        Task UpdateVersionConfigurationAsync(Guid versionId, UpdateTitleVersionConfigurationRequest updateVersionConfigReq);
    }
}