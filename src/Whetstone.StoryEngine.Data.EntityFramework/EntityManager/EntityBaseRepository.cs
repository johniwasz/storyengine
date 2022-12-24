using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models.Data;


namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public class EntityBaseRepository
    {
        protected readonly IUserContextRetriever UserContextRetriever;
        protected readonly ITitleCacheRepository TitleCacheRep;

        public EntityBaseRepository(IUserContextRetriever userContextRetriever,
            ITitleCacheRepository titleCacheRep)
        {
            UserContextRetriever =
                userContextRetriever ?? throw new ArgumentNullException(nameof(userContextRetriever));
            TitleCacheRep =
                titleCacheRep ?? throw new ArgumentNullException(nameof(titleCacheRep));
        }



        protected async Task<Guid?> GetTitleDataIdAsync(string titleId)
        {
            Guid? dataTitleId = null;
            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                DataTitle foundTitle = await userContext.Titles.Where(x =>
                            x.ShortName.ToLower().Equals(titleId.ToLower())).SingleOrDefaultAsync();

                if (foundTitle != null)
                    dataTitleId = foundTitle.Id;
            }

            return dataTitleId;
        }


        /// <summary>
        /// Gets the version from the database given the title short name and version. Returns the version
        /// if it is not deleted.
        /// </summary>
        /// <param name="dbOptions">Database options</param>
        /// <param name="titleName">Short name of the title.</param>
        /// <param name="version">Version to retrieve</param>
        /// <returns>An undeleted version if it exists. Null if it does not.</returns>
        protected async Task<DataTitleVersion> GetVersionDataAsync(
            string titleName, string version)
        {
            return await GetVersionDataAsync(titleName, version, false);
        }




        protected async Task<DataTitleVersion> GetVersionDataAsync(
                Guid versionId, bool includeDeleted)
        {
            DataTitleVersion dataVersion = null;

            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {

                if (includeDeleted)
                {
                    dataVersion = await userContext.TitleVersions
                        .Where(tv => tv.Id.Equals(versionId))
                        .SingleOrDefaultAsync();
                }
                else
                {

                    dataVersion = await userContext.TitleVersions
                       .Where(tv => tv.Id.Equals(versionId)
                       && !tv.IsDeleted)
                       .SingleOrDefaultAsync();
                }
            }
            return dataVersion;
        }

        protected async Task<DataTitleVersion> GetVersionDataAsync(
           string titleName, string version, bool includeDeleted)
        {
            DataTitleVersion dataVersion = null;

            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {

                if (includeDeleted)
                {
                    dataVersion = await userContext.TitleVersions.Join(userContext.Titles,
                            tv => tv.TitleId,
                            t => t.Id,
                            (tv, t) => new { Title = t, TitleVerion = tv })
                        .Where(t => t.Title.ShortName.ToLower().Equals(titleName.ToLower())
                                    && t.TitleVerion.Version.ToLower().Equals(version.ToLower()))
                        .Select(t => t.TitleVerion)
                        .SingleOrDefaultAsync();
                }
                else
                {
                    // Get the data title version if it is not marked as deleted and the 
                    // associated title matches the parent title.
                    dataVersion = await userContext.TitleVersions.Join(userContext.Titles,
                            tv => tv.TitleId,
                            t => t.Id,
                            (tv, t) => new { Title = t, TitleVerion = tv })
                        .Where(t => t.Title.ShortName.ToLower().Equals(titleName.ToLower())
                                    && t.TitleVerion.Version.ToLower().Equals(version.ToLower())
                                    && !t.TitleVerion.IsDeleted)
                        .Select(t => t.TitleVerion)
                        .SingleOrDefaultAsync();
                }
            }



            return dataVersion;
        }

        protected async Task<DataTitle> CreateOrUpdateTitleAsync(string titleShortName,
            string titleLongName, string description)
        {

            return await CreateOrUpdateTitleAsync(null, titleShortName, titleLongName, description);

        }


        protected async Task<DataTitle> CreateOrUpdateTitleAsync(Guid? dataTitleId,
            string titleShortName, string titleLongName, string description)
        {


            if (string.IsNullOrEmpty(titleShortName))
                throw new ArgumentNullException(nameof(titleShortName));

            if (string.IsNullOrWhiteSpace(titleLongName))
                throw new ArgumentException(nameof(titleLongName));

            DataTitle dataTitle = new DataTitle
            {
                Description = description,
                ShortName = titleShortName,
                Title = titleLongName
            };



            if (dataTitleId.HasValue)
                dataTitle.Id = dataTitleId;
            else
                dataTitle.Id = await GetTitleDataIdAsync(dataTitle.ShortName);

            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                userContext.Titles.AddOrUpdate<DataTitle>(dataTitle);

                await userContext.SaveChangesAsync();
            }

            return dataTitle;
        }

        protected async Task DeleteTitleAsync(string environment, Guid? dataTitleId)
        {
            if (string.IsNullOrWhiteSpace(environment))
                throw new ArgumentNullException(nameof(environment));

            if (!dataTitleId.HasValue)
                throw new ArgumentNullException(nameof(dataTitleId));


            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                DataTitle dt = new DataTitle { Id = dataTitleId };
                userContext.Titles.Attach(dt);
                userContext.Titles.Remove(dt);
                await userContext.SaveChangesAsync();
            }

        }
    }
}
