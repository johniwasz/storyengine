using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Npgsql;
using System.Linq;
using System;
using System.Collections.Generic;
using Whetstone.StoryEngine.Data;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Data;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Data.Caching;
using System.IO;
using System.IO.Compression;
using System.Linq.Expressions;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Serialization;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public class DataTitleVersionRepository : EntityBaseRepository, IStoryVersionRepository
    {

        private readonly IFileRepository _fileRep = null;

        public DataTitleVersionRepository( IUserContextRetriever userContextRetriever,
            ITitleCacheRepository titleCacheRep,
            IFileRepository fileRep) : base(userContextRetriever, titleCacheRep)
        {
            _fileRep = fileRep ?? throw new ArgumentNullException(nameof(fileRep));
        }


        

        public async Task CreateOrUpdateVersionAsync(StoryTitle title)
        {
         

            if (title == null)
                throw new ArgumentNullException(nameof(title));

            string titleName = title.Id;

            if (string.IsNullOrEmpty(titleName))
                throw new ArgumentNullException(nameof(title), "TitleName cannot be null");

            string version = title.Version;

            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(title), "Version cannot be null or empty");

            // Create the title if it doesn't exist
            Guid dataTitleId;

            // If the data title id is found in the StoryTitle, then it doesn't need to be created in the database.
            if (!title.DataTitleId.HasValue)
            {

                Guid? foundTitleId = await GetTitleDataIdAsync(titleName);

                if (!foundTitleId.HasValue)
                {
                    // if the title is not found, create it.
                    var titleInfo = await CreateOrUpdateTitleAsync(titleName, title.Title, title.Description);
                    dataTitleId = titleInfo.Id.Value;
                }
                else
                {
                    dataTitleId = foundTitleId.Value;
                }
            }
            else
            {
                dataTitleId = title.DataTitleId.Value;
            }

            // Update the version in the database.
            await CreateOrUpdateSimpleVersionAsync(dataTitleId, titleName, title.Version, title.Description);

            // Update the story in cache.
            await TitleCacheRep.SetTitleVersionAsync(title);

            // Update the version in S3 storage.
            await _fileRep.StoreTitleAsync(title);


        }

        private async Task CreateOrUpdateSimpleVersionAsync(Guid titleId, string titleName, string version, string description)
        {
            if(string.IsNullOrWhiteSpace(titleName))
                throw new ArgumentNullException(nameof(titleName));


            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));


         
            DataTitleVersion dataVersion = await GetVersionDataAsync(titleName, version);
            if (dataVersion != null)
            {
                dataVersion.Description = description;
            }
            else
            {
                dataVersion = new DataTitleVersion();
                dataVersion.Version = version;
                dataVersion.TitleId = titleId;
                dataVersion.Description = description;
            }

            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                userContext.TitleVersions.AddOrUpdate<DataTitleVersion>(dataVersion);

                await userContext.SaveChangesAsync();
            }
            // This process does not affect the logging flag. There's no need to propagate changes.

        }

        public async Task PublishVersionAsync(PublishVersionRequest publishRequest)
        {

            if (publishRequest == null)
                throw new ArgumentNullException(nameof(publishRequest));

            if (string.IsNullOrWhiteSpace(publishRequest?.ClientId))
                throw new ArgumentNullException(nameof(publishRequest), "ClientId cannot be null or empty");

            if (string.IsNullOrWhiteSpace(publishRequest?.TitleName))
                throw new ArgumentNullException(nameof(publishRequest), "TitleId cannot be null or empty");

            if (string.IsNullOrWhiteSpace(publishRequest?.Version))
                throw new ArgumentNullException(nameof(publishRequest), "Version cannot be null or empty");

            // Alias can be null or empty



            Guid? dataTitleId = await GetTitleDataIdAsync(publishRequest.TitleName);

            if (dataTitleId.HasValue)
            {

                // The title exists. Now we will check if the version exists. 
                DataTitleVersion dataVersion = await GetVersionDataAsync( publishRequest.TitleName, publishRequest.Version);

                Guid dataVersionId;
                if (dataVersion == null)
                {
                    // Create the version since it does not exist.
                    dataVersion = new DataTitleVersion();
                    dataVersion.TitleId = dataTitleId.Value;
                    dataVersion.Version = publishRequest.Version;

                    try
                    {
                      
                        dataVersionId = await CreateOrUpdateVersionAsync(dataVersion);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Cannot publish version {publishRequest.Version} for client {publishRequest.ClientType} with client identifier {publishRequest.ClientId}: could not create version",
                            ex);
                    }
                }
                else
                {
                    dataVersionId = dataVersion.Id.Value;

                }

                string clientAlias = string.IsNullOrWhiteSpace(publishRequest.Alias) ? null : publishRequest.Alias;
                // Get the existing deployment, if there is one.
                Guid? existingDeploymentId = null;
                try
                {

                 
                    using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
                    {

                        DataTitleVersionDeployment deployment = await userContext.TitleVersionDeployments.Where(x =>
                                !x.IsDeleted && x.Client == publishRequest.ClientType
                                             && x.ClientIdentifier.ToLower().Equals(publishRequest.ClientId.ToLower())
                                             && x.VersionId == dataVersionId
                                             && x.Alias == clientAlias)
                            .SingleOrDefaultAsync();

                        if (deployment != null)
                            existingDeploymentId = deployment.Id;

                    }

                }
                catch (Exception ex)
                {
                    string errMessage = string.IsNullOrWhiteSpace(clientAlias)
                        ? $"Error looking for deployment for client {publishRequest.ClientType} version {publishRequest.Version} and title {publishRequest.TitleName}"
                        : $"Error looking for deployment for client {publishRequest.ClientType} version {publishRequest.Version} and title {publishRequest.TitleName} and alias {clientAlias}";


                    throw new Exception(
                        errMessage,
                        ex);
                }

                DataTitleVersionDeployment newDeployment = null;
                // Add a new deployment
                try
                {
                    using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
                    {
                        newDeployment = new DataTitleVersionDeployment();
                        newDeployment.Client = publishRequest.ClientType;
                        newDeployment.ClientIdentifier = publishRequest.ClientId;
                        newDeployment.VersionId = dataVersionId;
                        newDeployment.PublishDate = DateTime.UtcNow;
                        newDeployment.IsDeleted = false;
                        newDeployment.Alias = clientAlias;
                        newDeployment.Id = existingDeploymentId;

                        userContext.TitleVersionDeployments.AddOrUpdate(newDeployment);
                        await userContext.SaveChangesAsync();
                    }


                }
                catch (Exception ex)
                {
                    throw new Exception(
                        $"Cannot deploy version {publishRequest.Version} for client {publishRequest.ClientType} with client identifier {publishRequest.ClientId}",
                        ex);
                }

                if (newDeployment?.Id != null)
                {
                    await UpdateAppMappingAsync(publishRequest.ClientType,publishRequest.ClientId, newDeployment.Alias,
                        newDeployment.Id.Value);

                }
            }
            else
                throw new Exception(
                    $"Cannot publish version {publishRequest.Version} for client {publishRequest.ClientType} with client identifier {publishRequest.ClientId}: {publishRequest.TitleName} not found");

        }

        private async Task UpdateAppMappingAsync(Client clientType, string clientAppId, string alias, Guid deploymentId)
        {

            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentNullException(nameof(clientAppId));

            if (deploymentId == default(Guid))
                throw new ArgumentNullException(nameof(deploymentId));

            TitleVersion retTitle = null;

            Expression<Func<TitleVersionCombo, bool>> dtvFunc = null;

            if (string.IsNullOrWhiteSpace(alias))
            {
                dtvFunc = (TitleVersionCombo x) => !x.TitleVersionDeployment.IsDeleted &&
                               x.TitleVersionDeployment.Id.Equals(deploymentId) &&
                               x.TitleVersionDeployment.Alias == null;

            }
            else
            {
                dtvFunc = x => !x.TitleVersionDeployment.IsDeleted &&
                               x.TitleVersionDeployment.Id.Equals(deploymentId) &&
                               x.TitleVersionDeployment.Alias.Equals(alias);

            }


           
            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {

            

                var titleFound = await userContext.TitleVersionDeployments.Join(userContext.TitleVersions,
                        dtv => dtv.VersionId,
                        tv => tv.Id,
                       (dtv, tv) => new TitleVersionCombo () { TitleVersionDeployment = dtv, TitleVersion = tv})
                    .Where(dtvFunc)
                    .Join(userContext.Titles,
                        dtv_tv => dtv_tv.TitleVersion.TitleId,
                        t => t.Id,
                        (dtv_tv, t) => new
                        {
                            dtv_tv.TitleVersionDeployment,
                            dtv_tv.TitleVersion,
                            Title = t
                        })
                    .Select(t => new
                    {
                        t.Title.ShortName,
                        t.TitleVersion.Version,
                        DeploymentId = t.TitleVersionDeployment.Id,
                        TitleId = t.Title.Id,
                        VersionId = t.TitleVersion.Id,
                        VersionDeploymentDate = t.TitleVersionDeployment.PublishDate,
                        t.TitleVersionDeployment.Alias,
                        LogClientFullMessage = t.TitleVersion.LogFullClientMessages
                    }).SingleOrDefaultAsync();

                if (titleFound != null)
                {
                    retTitle = new TitleVersion
                    {
                        TitleId = titleFound.TitleId,
                        VersionId = titleFound.VersionId,
                        DeploymentId = titleFound.DeploymentId,
                        ShortName = titleFound.ShortName,
                        Version = titleFound.Version,
                        Alias = titleFound.Alias,
                        LogFullClientMessages = titleFound.LogClientFullMessage
                    };

                    // Update the title version in the cache.
                    await UpdateAppMappingAsync(clientType, clientAppId,  retTitle);
                }
            }
        }



        private async Task UpdateAppMappingAsync(Client clientType, string clientAppId, TitleVersion titleVersion)
        {
            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentNullException(nameof(clientAppId));

            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));


            await TitleCacheRep.SetAppMappingAsync(clientType, clientAppId, titleVersion);
        }

        private async Task<Guid> CreateOrUpdateVersionAsync(DataTitleVersion titleVersion)
        {
            if (titleVersion == null)
                throw new ArgumentNullException(nameof(titleVersion));

            try
            {
                using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    userContext.TitleVersions.AddOrUpdate(titleVersion);
                    await userContext.SaveChangesAsync();
                }
            }
            catch(DbUpdateException updateEx )
            {
                if(updateEx.InnerException == null)
                {
                    throw;
                }

                if(updateEx.InnerException is PostgresException)
                {
                    PostgresException postEx = (PostgresException)updateEx.InnerException;
                    if (postEx.SqlState.Equals(UserDataContext.POSTGESQL_CODE_DUPLICATEKEY))
                    {
                        throw new DuplicateKeyException($"Duplicate version {titleVersion.Version} in title {titleVersion.TitleId}", postEx);
                    }

                }
               
                throw;
            }

            // TODO Push updates to the title version representations in the cache.





            return titleVersion.Id.Value;
        }

        public async Task<List<DataTitleVersionDeployment>> GetDeploymentsAsync(Guid versionId)
        {
            List<DataTitleVersionDeployment> deployments= null;
            try
            {
                using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    deployments =
                        await userContext.TitleVersionDeployments.Where(x => !x.IsDeleted && x.VersionId == versionId)
                            .ToListAsync();
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting deployments for version {versionId}", ex);
            }

            return deployments;
        }


        private async Task<Guid?> GetVersionDataIdAsync(DbContextOptions<UserDataContext> dbOptions, Guid titleId, string version)
        {
            Guid? dataVersionId = null;
            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                DataTitleVersion foundVersion = await userContext.TitleVersions.SingleOrDefaultAsync(x =>                 
                x.Version.ToLower().Equals(version.ToLower()));

                if (foundVersion != null)
                    dataVersionId = foundVersion.Id;
            }
            return dataVersionId;
        }



        public async Task DeleteVersionAsync(string titleName, string version)
        {

            if (string.IsNullOrWhiteSpace(titleName))
                throw new ArgumentNullException(nameof(titleName));

            if(string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));


            DataTitleVersion dataTitle = await GetVersionDataAsync(titleName, version);

            await DeleteTitleVersionCacheAsync(titleName, version);

            TitleVersion titleVer = new TitleVersion(titleName, version);

            await _fileRep.DeleteTitleAsync( titleVer);

            await TitleCacheRep.RemoveTitleVersionAsync(titleVer, true);

            if (dataTitle!=null)
            {
              
                using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    dataTitle.IsDeleted = true;
                    dataTitle.DeleteDate = DateTime.UtcNow;
                    userContext.TitleVersions.AddOrUpdate(dataTitle);
                     await userContext.SaveChangesAsync();
                }
            }
            else
                throw new TitleNotFoundException($"TitleId {titleName} for version {version} not found");


        }

        private async Task DeleteTitleVersionCacheAsync(string titleShortName, string version)
        {
            using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
            {

                var foundDeployments =  userContext.TitleVersionDeployments.Join(userContext.TitleVersions,
                        dtv => dtv.VersionId,
                        tv => tv.Id,
                        (dtv, tv) => new {TitleVersionDeployment = dtv, TitleVerion = tv})
                    .Where(dtv_tv => !dtv_tv.TitleVersionDeployment.IsDeleted)
                    .Join(userContext.Titles,
                        dtv_tv => dtv_tv.TitleVerion.TitleId,
                        t => t.Id,
                        (dtv_tv, t) => new
                        {
                            dtv_tv.TitleVersionDeployment,
                            TitleVersion = dtv_tv.TitleVerion,
                            Title = t
                        }).Where(dtv_tv =>
                        dtv_tv.Title.ShortName.Equals(titleShortName) && dtv_tv.TitleVersion.Version.Equals(version))
                    .Select(t => new
                    {
                        DeploymentId = t.TitleVersionDeployment.Id,
                        t.Title.ShortName,
                        t.TitleVersion.Version,
                        t.TitleVersionDeployment.Client,
                        t.TitleVersionDeployment.ClientIdentifier,
                        t.TitleVersionDeployment.Alias
                    });

                if ((foundDeployments?.Any()).GetValueOrDefault(false))
                {
                    // Remove the found deployment cache mappings from the cache
                    foreach (var foundDeployment in foundDeployments)
                    {
                        await TitleCacheRep.RemoveAppMappingAsync(foundDeployment.Client, foundDeployment.ClientIdentifier,
                            foundDeployment.Alias, true);
                    }



                }

            }

        }

        public async Task<StoryTitle> CloneVersionAsync(string titleId, string sourceVersion, string destVersion)
        {


            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            if (string.IsNullOrWhiteSpace(destVersion))
                throw new ArgumentNullException(nameof(destVersion));



            StoryTitle sourceTitle = null;
            TitleVersion sourceVer = null;
           
            if (string.IsNullOrWhiteSpace(sourceVersion))
            {
                // Get the root version
                sourceVer = new TitleVersion(titleId, null);               
            }
            else
            {
                sourceVer = new TitleVersion(titleId, sourceVersion);
            }

            try
            {
                sourceTitle = await _fileRep.GetTitleContentsAsync(sourceVer);
            }
            catch(Exception ex)
            {
                throw new TitleNotFoundException($"Source title {titleId} and version {sourceVersion} not found", ex);
            }

            // Create the title if it doesn't already exist.
            DataTitle dataTitle =  await CreateOrUpdateTitleAsync(sourceTitle.DataTitleId, sourceTitle.Id, sourceTitle.Title, sourceTitle.Description);

            sourceTitle.DataTitleId = dataTitle.Id;


            // Create or update the data version

  
            DataTitleVersion sourceDataTitleVersion = await GetVersionDataAsync(titleId, sourceVersion);
            string versionDescription = null;


            // Apply the version description to indicate it was cloned
            string sourceVersionText =string.IsNullOrWhiteSpace(sourceVersion) ? "root" : sourceVersion;


            if (sourceDataTitleVersion == null)
            {
            
                versionDescription = $"Cloned from {sourceVersionText}";
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(sourceDataTitleVersion.Description))
                {
                    versionDescription = $"{sourceDataTitleVersion.Description} (cloned from {sourceVersionText})";
                }
                else
                    versionDescription = $"Cloned from {sourceVersionText}";
            }

            // Does the version exist?
            TitleVersion destVer = new TitleVersion(titleId, destVersion);
           
            DataTitleVersion titleVersion = await GetVersionDataAsync( titleId, destVersion);
            Guid versionGuid;

            if (titleVersion == null)
            {
                DataTitleVersion destDataVersion = new DataTitleVersion();
                destDataVersion.TitleId = dataTitle.Id.Value;
                destDataVersion.Version = destVersion;
                destDataVersion.Description = versionDescription;

            
                versionGuid = await CreateOrUpdateVersionAsync(destDataVersion);
            }
            else
                versionGuid = titleVersion.Id.Value;

            sourceTitle.DataVersionTitleId = versionGuid;


            // Save the source title to the file location
            sourceTitle.Version = destVersion;

            await _fileRep.StoreTitleAsync( sourceTitle);

            // Copy audio and image files.
            await _fileRep.CopyMediaFilesAsync( sourceTitle.Id, sourceVersion, destVersion);

            // Add the cloned title to cache
            await TitleCacheRep.SetTitleVersionAsync(sourceTitle);

            return sourceTitle;


        }

        public async Task PurgeVersionAsync(string titleName, string version)
        {

            if (string.IsNullOrWhiteSpace(titleName))
                throw new ArgumentNullException(nameof(titleName));


            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));

            // remove versions and deployments in the database.      

            DataTitleVersion foundVersion = await GetVersionDataAsync( titleName, version, true);

            if (foundVersion != null)
            {           
                using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    userContext.TitleVersionDeployments.RemoveRange(
                        userContext.TitleVersionDeployments.Where(x => x.VersionId == foundVersion.Id));

                    userContext.TitleVersions.Remove(foundVersion);

                    await userContext.SaveChangesAsync();
                }
            }

            TitleVersion titleVer = new TitleVersion(titleName, version);

            await _fileRep.PurgeTitleAsync(titleVer);


        }


        public async Task<byte[]> ExportToZip( TitleVersion titleVer)
        {
            StoryTitle title = await _fileRep.GetTitleContentsAsync( titleVer);

            if (title == null)
                throw new TitleNotFoundException($"Title {titleVer.ShortName} and version {titleVer.Version}");

            var ser = YamlSerializationBuilder.GetYamlSerializer();

            string rawText = ser.Serialize(title);

            List<string> files = GetFiles(title);
            byte[] retArry = null;

            using (MemoryStream memStr = new MemoryStream())
            {
                using (ZipArchive archive = new ZipArchive(memStr, ZipArchiveMode.Create, true))
                {
                    ZipArchiveEntry globalEntry = archive.CreateEntry($"{titleVer.ShortName}.yaml");
                    using (StreamWriter writer = new StreamWriter(globalEntry.Open()))
                    {
                        writer.Write(rawText);
                    }

                    // Check all the nodes in the global config and store any associated 
                    // media.
                    foreach (string file in files)
                    {
                        byte[] outBinFile = await _fileRep.GetFileContentAsync(titleVer, file);
                        if (outBinFile != null)
                        {
                            ZipArchiveEntry binEntry = archive.CreateEntry(file);
                            using (BinaryWriter writer = new BinaryWriter(binEntry.Open()))
                            {
                                writer.Write(outBinFile);
                            }
                        }
                    }
                }

                memStr.Position = 0;
                retArry = memStr.ToArray();


            }

            return retArry;
        }

        private List<string> GetFiles(StoryTitle title)
        {
            List<string> foundFiles = new List<string>();
            if (title.Nodes != null)
            {
                foreach (StoryNode node in title.Nodes)
                {

                    foundFiles.AddRange(node.FindExportFiles());
                }
            }

            return foundFiles.Distinct().ToList();
        }

        public async Task<TitleVersion> GetTitleVersionByDeploymentIdAsync(Guid deploymentId)
        {


            if (deploymentId == default(Guid))
                throw new ArgumentNullException(nameof(deploymentId));

            TitleVersion retTitleVer = null;

            using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                var foundDeployment =
                  await   userContext.TitleVersionDeployments.Where(tvd => tvd.Id.Equals(deploymentId) && !tvd.IsDeleted)
                        .Join(userContext.TitleVersions,
                            tvd => tvd.VersionId,
                            tv => tv.Id,
                            (tvd, tv) => new
                            {
                                TitleVersionId = tv.Id,  tv.TitleId, DeploymentId = tvd.Id,  tvd.Alias,
                                tvd.Client
                            })
                        .Join(userContext.Titles, joinObj => joinObj.TitleId, t => t.Id,
                            (joinObj, t) => 
                                new
                                    { TitleId = t.Id.Value, ShortName = t.Title, joinObj.Alias,  joinObj.DeploymentId, VersionId = joinObj.TitleVersionId.Value }).SingleOrDefaultAsync();


                if (foundDeployment != null)
                {
                    retTitleVer = new TitleVersion
                    {
                        Alias = foundDeployment.Alias,
                        DeploymentId = foundDeployment.DeploymentId,
                        VersionId = foundDeployment.VersionId,
                        TitleId = foundDeployment.TitleId,
                        ShortName = foundDeployment.ShortName
                    };

                }
            }

            return retTitleVer;

        }

        


        public async Task<TitleVersionConfiguration> GetVersionConfigurationAsync(string titleId, string version)
        {
            return await GetVersionConfigurationAsync(titleId, version, false);
        }


        /// <summary>
        /// Returns the configuration settings of the version as it exists in the database.
        /// </summary>
        /// <param name="titleId"></param>
        /// <param name="version"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<TitleVersionConfiguration> GetVersionConfigurationAsync(string titleId, string version, bool includeDeleted)
        {
            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));

            TitleVersionConfiguration titleConfig = new TitleVersionConfiguration();

            try
            {
              
                using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    var foundVersion =
                        await userContext.Titles.Where(x => x.ShortName.Equals(titleId))
                            .Join(userContext.TitleVersions,
                                t => t.Id,
                                tv => tv.TitleId,
                                (t, tv) => new
                                {
                                    TitleId = t.Id,
                                    TitleName = t.ShortName,
                                    tv.IsDeleted,
                                    tv.LogFullClientMessages,
                                    VersionId = tv.Id,
                                    tv.Description,
                                    tv.DeleteDate,
                                    tv.Version

                                }).Where(x=> x.Version.Equals(version))
                            .SingleOrDefaultAsync();


                    if (foundVersion != null)
                    {
                        if (includeDeleted || (!includeDeleted && !foundVersion.IsDeleted))
                        {
                            titleConfig.IsDeleted = foundVersion.IsDeleted;
                            titleConfig.TitleId = foundVersion.TitleId;
                            titleConfig.VersionId = foundVersion.VersionId;
                            titleConfig.Description = foundVersion.Description;
                            titleConfig.LogFullClientMessages = foundVersion.LogFullClientMessages;
                            titleConfig.Version = foundVersion.Version;
                            titleConfig.DeletedDate = foundVersion.DeleteDate;
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting version configuration for title {titleId} and version {version}", ex);
            }

            return titleConfig;

        }


        public async Task<TitleVersionConfiguration> UpdateVersionConfigurationAsync(string titleName, string version,
            UpdateTitleVersionConfigurationRequest updateVersionConfigReq)
        {
            if (string.IsNullOrWhiteSpace(titleName))
                throw new ArgumentNullException(nameof(titleName));

            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));

            TitleVersionConfiguration titleVerConfig = null;

            if (updateVersionConfigReq == null)
                throw new ArgumentNullException(nameof(updateVersionConfigReq));

          
            DataTitleVersion dataTitleVer = null;

            try
            {
                
                // Get the title version requests
                dataTitleVer = await GetVersionDataAsync(titleName, version);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error getting version {version} for title {titleName} while attempting to update the configuration",ex);
            }


            if (dataTitleVer != null)
            {
                bool isModified = false;
                if (!string.IsNullOrWhiteSpace(updateVersionConfigReq.Description))
                {
                    if (!dataTitleVer.Description.Equals(updateVersionConfigReq.Description))
                    {
                        dataTitleVer.Description = updateVersionConfigReq.Description;
                        isModified = true;
                    }
                }

                bool isLogFullClientMessagesUpdated = false;
                if (updateVersionConfigReq.LogFullClientMessages.HasValue)
                {
                    if (dataTitleVer.LogFullClientMessages != updateVersionConfigReq.LogFullClientMessages.Value)
                    {
                        dataTitleVer.LogFullClientMessages = updateVersionConfigReq.LogFullClientMessages.Value;
                        isLogFullClientMessagesUpdated = true;
                        isModified = true;
                    }
                }

                if (isModified)
                {
                    try
                    {
                       
                        using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                        {
                            userContext.TitleVersions.Attach(dataTitleVer);

                            userContext.Entry(dataTitleVer).State = EntityState.Modified;

                            await userContext.SaveChangesAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Error updating version {version} for title {titleName} while attempting to update the configuration", ex);
                    }
                }

                if (isLogFullClientMessagesUpdated)
                {
                    // Update all the cached title version deployments that are related to this title version.
                    // Get all deployments for this version
                    List<DataTitleVersionDeployment> versionDeployments = null;
                    try
                    {
                      
                        using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                        {
                            versionDeployments =
                                await userContext.TitleVersionDeployments.Where(x => x.VersionId.Equals(dataTitleVer.Id))
                                    .ToListAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            $"Error updating cached app mappings for version {version} for title {titleName} while attempting to update the configuration", ex);

                    }

                    if ((versionDeployments?.Any()).GetValueOrDefault(false))
                    {
                        foreach (var deployment in versionDeployments)
                        {
                            TitleVersion refreshedTitleVer = new TitleVersion
                            {
                                TitleId = dataTitleVer.TitleId,
                                VersionId = dataTitleVer.Id,
                                DeploymentId = deployment.Id,
                                ShortName = titleName,
                                Version = dataTitleVer.Version,
                                Alias = deployment.Alias,
                                LogFullClientMessages = dataTitleVer.LogFullClientMessages
                            };

                            // Update the title version in the cache.
                            await UpdateAppMappingAsync(deployment.Client, deployment.ClientIdentifier,
                                refreshedTitleVer);
                        }
                    }
                }
            }


            titleVerConfig = new TitleVersionConfiguration
            {
                Description = dataTitleVer.Description,
                IsDeleted = dataTitleVer.IsDeleted,
                LogFullClientMessages = dataTitleVer.LogFullClientMessages,
                DeletedDate = dataTitleVer.DeleteDate,
                TitleId = dataTitleVer.TitleId,
                VersionId = dataTitleVer.Id,
                TitleName = titleName
            };

            return titleVerConfig;

        }

        public async Task UpdateVersionConfigurationAsync(Guid versionId, UpdateTitleVersionConfigurationRequest updateVersionConfigReq)
        {
            if (versionId == default(Guid))
                throw new ArgumentNullException(nameof(versionId));

            if (updateVersionConfigReq == null)
                throw new ArgumentNullException(nameof(updateVersionConfigReq));

            DataTitleVersion dataTitleVer;

            try
            {

                // Get the title version requests
                dataTitleVer = await GetVersionDataAsync(versionId, false);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error getting version {versionId} while attempting to update the configuration", ex);
            }


            if (updateVersionConfigReq.Description != null)
                dataTitleVer.Description = updateVersionConfigReq.Description;

            if (updateVersionConfigReq.LogFullClientMessages.HasValue)
                dataTitleVer.LogFullClientMessages = updateVersionConfigReq.LogFullClientMessages.Value;


            try
            {

                using (var userContext = await UserContextRetriever.GetUserDataContextAsync())
                {
                    userContext.TitleVersions.Attach(dataTitleVer);

                    userContext.Entry(dataTitleVer).State = EntityState.Modified;

                    await userContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Error updating version {versionId} while attempting to update the configuration", ex);
            }


        }
    }

    internal class TitleVersionCombo
    {

        internal DataTitleVersionDeployment TitleVersionDeployment { get; set; }


        internal DataTitleVersion TitleVersion { get; set; }

    }
}
