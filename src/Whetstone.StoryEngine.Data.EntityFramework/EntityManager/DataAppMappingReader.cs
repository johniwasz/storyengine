using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Story;

namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public class DataAppMappingReader : EntityBaseRepository, IAppMappingReader
    {

        private readonly ILogger<DataAppMappingReader> _dataLogger;


        public DataAppMappingReader(IUserContextRetriever userContextRetriever,
            ITitleCacheRepository titleCacheRep, ILogger<DataAppMappingReader> logger) : base(userContextRetriever, titleCacheRep)
        {
            _dataLogger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }



        public async Task<TitleVersion> GetTitleAsync( Client clientType, string appId, string alias)
        {


            if (string.IsNullOrWhiteSpace(appId))
                throw new ArgumentException($"{nameof(appId)} cannot be null or empty");

            TitleVersion retTitle = null;
            string container =GetCacheContainer();
            TitleVersion mappedTitle = await TitleCacheRep.GetAppMappingAsync(clientType, appId, alias);

            if (mappedTitle != null)
            {

                retTitle = mappedTitle;
            }
            else
            {
                TitleVersion foundTitle = await GetTitleFromDatabaseAsync(clientType, appId, alias);
                if (foundTitle != null)
                {
                    retTitle = foundTitle;

                    await TitleCacheRep.SetAppMappingAsync(clientType, appId, foundTitle);
                }
            }

            return retTitle;
        }


        private string GetCacheContainer()
        {

            return "dataappmappings";

        }

        private async Task<TitleVersion> GetTitleFromDatabaseAsync( Client clientType, string clientAppId, string alias)
        {

            if (string.IsNullOrWhiteSpace(clientAppId))
                throw new ArgumentException($"{nameof(clientAppId)} cannot by null or empty");


            TitleVersion retTitle = null;


            using (IUserDataContext userContext = await UserContextRetriever.GetUserDataContextAsync())
            {
                var titleFound = userContext.TitleVersionDeployments.Join(userContext.TitleVersions,
                                         dtv => dtv.VersionId,
                                         tv => tv.Id,
                                         (dtv, tv) => new { TitleVersionDeployment = dtv, TitleVerion = tv })
                                         .Where(dtv_tv => !dtv_tv.TitleVersionDeployment.IsDeleted &&
                                                         dtv_tv.TitleVersionDeployment.ClientIdentifier.ToLower().Equals(clientAppId.ToLower()) &&
                                                         dtv_tv.TitleVersionDeployment.Client == clientType)
                                         .Join(userContext.Titles,
                                           dtv_tv => dtv_tv.TitleVerion.TitleId,
                                           t => t.Id,
                                           (dtv_tv, t) => new
                                           {
                                               TitleVersionDeployment = dtv_tv.TitleVersionDeployment,
                                               TitleVersion = dtv_tv.TitleVerion,
                                               Title = t
                                           })
                                        .Select(t => new {
                                            t.Title.ShortName,
                                            t.TitleVersion.Version,
                                            DeploymentId = t.TitleVersionDeployment.Id,
                                            TitleId = t.Title.Id,
                                            VersionId = t.TitleVersion.Id,
                                            VersionDeploymentDate = t.TitleVersionDeployment.PublishDate,
                                            Alias = t.TitleVersionDeployment.Alias,
                                            LogClientFullMessage = t.TitleVersion.LogFullClientMessages
                                        })
                                        .OrderByDescending(t => t.VersionDeploymentDate);

                if ((titleFound?.Any()).GetValueOrDefault(false))
                {


                    var aliasTitle = await titleFound.FirstOrDefaultAsync(x =>
                        !string.IsNullOrWhiteSpace(x.Alias) &&
                        x.Alias.ToLower().Equals(alias.ToLower()));
                    if (aliasTitle != null)
                    {

                        retTitle = new TitleVersion
                        {
                            TitleId = aliasTitle.TitleId,
                            VersionId = aliasTitle.VersionId,
                            DeploymentId = aliasTitle.DeploymentId,
                            ShortName = aliasTitle.ShortName,
                            Version = aliasTitle.Version,
                            Alias = aliasTitle.Alias,
                            LogFullClientMessages = aliasTitle.LogClientFullMessage
                        };

                    }



                    if (retTitle == null)
                    {
                        var firstFoundTitle = await titleFound.FirstAsync(x => string.IsNullOrWhiteSpace(x.Alias));
                        retTitle = new TitleVersion
                        {
                            TitleId = firstFoundTitle.TitleId,
                            VersionId = firstFoundTitle.VersionId,
                            DeploymentId = firstFoundTitle.DeploymentId,
                            ShortName = firstFoundTitle.ShortName,
                            Version = firstFoundTitle.Version,
                            Alias = firstFoundTitle.Alias,
                            LogFullClientMessages = firstFoundTitle.LogClientFullMessage
                        };
                    }




                }
            }

            if (retTitle == null)
            {
                StringBuilder noTitleMessage = new StringBuilder();
                noTitleMessage.Append(
                    $"No deployment found for client {clientType} and client app id {clientAppId}");

                if (!string.IsNullOrWhiteSpace(alias))
                    noTitleMessage.Append($" alias {alias}");

                throw new Exception(noTitleMessage.ToString());
            }


            StringBuilder logResultBuilder = new StringBuilder();
            logResultBuilder.Append($"Request from Client {clientType} client app id {clientAppId} ");
            if(string.IsNullOrWhiteSpace(alias))
                logResultBuilder.Append($"Request from Client {clientType} client app id {clientAppId} and null alias ");
            else
                logResultBuilder.Append($"Request from Client {clientType} client app id {clientAppId} and alias {alias} ");

            if (retTitle != null)
                logResultBuilder.Append(" is not mapped to a title version");
            else
            {
                logResultBuilder.Append(" resolved to title: ");
                logResultBuilder.AppendLine();
                logResultBuilder.AppendLine($" Title: {retTitle.ShortName}, ID {retTitle.TitleId}");
                logResultBuilder.AppendLine($" Version: {retTitle.Version}, ID {retTitle.VersionId}");
                logResultBuilder.AppendLine($" DeploymentId: {retTitle.DeploymentId}");

                if (!string.IsNullOrWhiteSpace(retTitle.Alias))
                    logResultBuilder.AppendLine($" Alias: {retTitle.Alias}");
            }


            _dataLogger.LogInformation(logResultBuilder.ToString());

            return retTitle;
        }

    }




}
