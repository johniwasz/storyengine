using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Data.Caching;
using Whetstone.StoryEngine.Data.EntityFramework.EntityManager;
using Whetstone.StoryEngine.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Data;
using System.Threading;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Data;

using ILogger = Amazon.Runtime.Internal.Util.ILogger;

namespace Whetstone.StoryEngine.Data.EntityFramework
{
    public class ProjectRepository : EntityBaseRepository, IProjectRepository
    {
        private const string ALEXASKILLID_PREFIX = "amzn1.ask.skill.";
        //EntityBaseRepository, ITitleRepository
        //{
        //private readonly ITitleReader _titleReader;
        //private readonly IFileRepository _fileRep;

        //public DataTitleRepository(IUserContextRetriever userContextRetriever,
        //    ITitleCacheRepository titleCacheRep, ITitleReader titleReader, IFileRepository fileRep) : base(userContextRetriever, titleCacheRep)
        //{
        //    _fileRep = fileRep;
        //    _titleReader = titleReader;
        //}

        private readonly ILogger<ProjectRepository> _logger;
       

       // TODO Use the IAuthorization service to validate project repository access
#pragma warning disable IDE0060 // Remove unused parameter
        public ProjectRepository(IUserContextRetriever userContextRetriever, ITitleCacheRepository titleCacheRep, ILogger<ProjectRepository> logger) 
            : base(userContextRetriever, titleCacheRep)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        
           
        }

        public Task<Project> GetProjectAsync(Guid id)
        {

            var tcs = new TaskCompletionSource<Project>();
            tcs.SetException(new NotImplementedException());
            return tcs.Task;


        }

        public async Task<IEnumerable<ProjectVersion>> GetVersionsAsync(Guid projectId)
        {
            List<ProjectVersion> projectVersions = new List<ProjectVersion>();


            ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

            Guid? userSid = prin?.GetUserSid();

            try
            {
                if (userSid.HasValue)
                {
                    await using var userContext = await UserContextRetriever.GetUserDataContextAsync();

                    var foundVersions =
                        userContext.TitleVersions.Where(x => x.TitleId.Equals(projectId) && !x.IsDeleted);

                    var titleFound = userContext.Titles
                        .Join(userContext.TitleGroupXRefs,
                            t => t.Id,
                            groupXref => groupXref.TitleId,
                            (t, groupXRef) => new
                            {
                                Title = t,
                                groupXRef.GroupId
                            }).Join(userContext.UserGroupXRefs,

                            groupTitle => groupTitle.GroupId,
                            userGroupXRef => userGroupXRef.GroupId,
                            (groupTitle, userGroupXRef) => new
                            {
                                groupTitle.Title,
                                userGroupXRef.UserId
                            })
                        .Join(userContext.TitleVersions,
                            t => t.Title.Id,
                            tv => tv.TitleId,
                            (t, tv) =>
                                new
                                {
                                    tv.IsDeleted,
                                    TItle = t.Title,
                                    Version = tv

                                })
                        .Where(x => x.TItle.Id.Equals(projectId) && !x.IsDeleted)
                        .Select(arg =>
                            new
                            {
                                arg.Version
                            });


                    await foundVersions.LoadAsync();

                    foreach (var version in foundVersions)
                    {
                        projectVersions.Add(version.ToProjectVersion(projectId));
                    }
                }
                else
                {
                    throw new ArgumentException($"Claim {ClaimTypes.Sid} does not contain a valid GUID", nameof(prin));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving versions for project id {projectId} for user {userSid}");
                throw;
            }

            return projectVersions;

        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            // validate that the user can get the current project.

            // _authService.AuthorizeAsync(System.Threading.Thread.CurrentPrincipal as ClaimsPrincipal, )

            ClaimsPrincipal prin = Thread.CurrentPrincipal as ClaimsPrincipal;

            if(prin == null)
            {
                throw new Exception("Principal not found");
            }

            List<Project> allProjects = new List<Project>();

            try
            {
                Guid? userSid = prin.GetUserSid();

                if(userSid.HasValue)
                { 
                    await using var userContext = await UserContextRetriever.GetUserDataContextAsync();

                    var titleFound = userContext.Titles
                        .Join(userContext.TitleGroupXRefs,
                            t => t.Id,
                            groupXref => groupXref.TitleId,
                            (t, groupXRef) => new
                            {
                                Title = t,
                                groupXRef.GroupId
                            }).Join(userContext.UserGroupXRefs,

                            groupTitle => groupTitle.GroupId,
                            userGroupXRef => userGroupXRef.GroupId,
                            (groupTitle, userGroupXRef) => new
                            {

                                groupTitle.Title,
                                userGroupXRef.UserId
                            })
                        .Where(arg => arg.UserId.Equals(userSid))
                        .Select(arg =>
                            new
                            {
                                arg.Title
                            });

                    await titleFound.LoadAsync();

                    foreach (var title in titleFound)
                    {

                        Project foundProject = new Project
                        {
                            Id = title.Title.Id,
                            Description = title.Title.Description,
                            ShortName = title.Title.ShortName
                        };

                        allProjects.Add(foundProject);
                    }
                }
                else
                {
                    throw new ArgumentException($"Claim {ClaimTypes.Sid} does not contain a valid GUID", nameof(prin));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all projects");
                throw;
            }

            return allProjects;
        }

        public async Task<IEnumerable<VersionDeployment>> GetVersionDeploymentsAsync(Guid projectId, Guid versionId)
        {
            List<VersionDeployment> versionDeployments = new List<VersionDeployment>();

            try
            {
                await using var userContext = await UserContextRetriever.GetUserDataContextAsync();

                var foundDeployments =
                    userContext.TitleVersionDeployments.Where(x => x.VersionId.Equals(versionId) && !x.IsDeleted);

                await foundDeployments.LoadAsync();

                foreach (var deployment in foundDeployments)
                {
                    versionDeployments.Add(deployment.ToVersionDeployment(projectId));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving deployments for project id {projectId} and versionId {versionId}");
                throw;
            }

            return versionDeployments;



        }

        public async Task<ProjectVersion> UpdateVersionAsync(Guid projectId, Guid versionId, ProjectVersionUpdateRequest updateRequest)
        {
            ProjectVersion retVersion = null;

            if (projectId == default)
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");

            if (versionId == default)
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");

            if (updateRequest == null)
                throw new ArgumentNullException(nameof(updateRequest));

            try
            {
                await using var userContext = await UserContextRetriever.GetUserDataContextAsync();

                DataTitleVersion versionFound = userContext.TitleVersions
                    .SingleOrDefault(x => x.TitleId.Equals(projectId) && !x.IsDeleted && x.Id.Value.Equals(versionId));


                if (versionFound != null)
                {

                    bool isUpdateRequested = false;
                    bool isLoggingFlagUpdated = false;

                    if (updateRequest.Description != null)
                    {
                        isUpdateRequested = true;
                        versionFound.Description = updateRequest.Description;
                    }

                    if (updateRequest.LogFullClientMessages.HasValue)
                    {
                      

                        if (versionFound.LogFullClientMessages != updateRequest.LogFullClientMessages)
                        {
                            isUpdateRequested = true;
                            isLoggingFlagUpdated = true;
                            versionFound.LogFullClientMessages = updateRequest.LogFullClientMessages.Value;

                            // Push logging changes to the cache.
                            
                        }
                    }

                    if (isUpdateRequested)
                        await userContext.SaveChangesAsync();

                    if (isLoggingFlagUpdated)
                        await UpdateDeploymentCacheAsync(userContext, versionId);


                    retVersion = versionFound.ToProjectVersion(projectId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Updating version {versionId} ");
                throw;
            }

            return retVersion;
        }


        /// <summary>
        /// Update the cached deployments with the new version settings.
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="versionId"></param>
        /// <returns></returns>
        private async Task UpdateDeploymentCacheAsync(IUserDataContext userContext, Guid versionId)
        {

            var foundItemItems = await (from tvd in userContext.TitleVersionDeployments
                join tv in userContext.TitleVersions on tvd.VersionId equals tv.Id
                join t in userContext.Titles on tv.TitleId equals t.Id
                where tv.Id == versionId && !tv.IsDeleted && !tvd.IsDeleted
                select new
                {
                    ProjectId = t.Id,
                    VersionId = tv.Id,
                    DeploymentId = tvd.Id,
                    IsVersionDeleted = tv.IsDeleted,
                    tv.LogFullClientMessages,
                    tv.Version,
                    ProjectName = t.ShortName,
                    tvd.Alias,
                    tvd.Client,
                    tvd.ClientIdentifier,
                }).ToListAsync();

            foreach (var foundItem in foundItemItems)
            {
                TitleVersion titleVer = new TitleVersion
                {
                    TitleId = foundItem.ProjectId,
                    VersionId = foundItem.VersionId,
                    Alias =foundItem.Alias,
                    LogFullClientMessages = foundItem.LogFullClientMessages,
                    DeploymentId = foundItem.DeploymentId,
                    Version = foundItem.Version,
                    ShortName = foundItem.ProjectName
                };


                await TitleCacheRep.SetAppMappingAsync(foundItem.Client, foundItem.ClientIdentifier, titleVer);
            }

            

        }

        public async Task<VersionDeployment> AddVersionDeploymentAsync(Guid projectId, Guid versionId,
            AddVersionDeploymentRequest deploymentRequest)
        {


            VersionDeployment retDeployment = null;

            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (deploymentRequest == null)
            {
                throw new ArgumentNullException(nameof(deploymentRequest));
            }

            if (deploymentRequest.ClientType == Client.Unknown)
            {
                throw new ArgumentException($"ClientType in {nameof(deploymentRequest)} cannot be {Client.Unknown}");
            }

            if (string.IsNullOrWhiteSpace(deploymentRequest.ClientId))
            {
                throw new ArgumentException($"ClientId in {nameof(deploymentRequest)} cannot be null or empty");
            }

            deploymentRequest.ClientId = deploymentRequest.ClientId.Trim();

            // Validate the client id format
            bool isClientIdValid = false;
            switch (deploymentRequest.ClientType)
            {
                case Client.Alexa:
                    isClientIdValid = ValidateAlexaClientId(deploymentRequest.ClientId);
                    break;
                case Client.Sms:
                    deploymentRequest.ClientId = FormatPhoneNumber(deploymentRequest.ClientId);
                    isClientIdValid = ValidateSmsClientId(deploymentRequest.ClientId);
                    break;
                case Client.Bixby:
                case Client.FacebookMessenger:
                case Client.MicrosoftInvoke:
                case Client.GoogleHome:
                    isClientIdValid = true;
                    // No known standard format.
                    break;
            }

            if (!isClientIdValid)
                throw new ArgumentException(
                    $"ClientId {deploymentRequest.ClientId} is an invalid format for client {deploymentRequest.ClientType}");



            try
            {
                await using var userContext = await UserContextRetriever.GetUserDataContextAsync();


               var foundItem = await  userContext.TitleVersions.Join(userContext.Titles, tv => tv.TitleId,
                    t => t.Id,
                    (tv, t) => new
                    {
                        ProjectId = t.Id.Value,
                        VersionId = tv.Id.Value,
                        ProjectName = t.ShortName,
                        tv.LogFullClientMessages,
                        tv.IsDeleted,
                        tv.Version
                    }).Where(joinedResult => 
                   !joinedResult.IsDeleted && 
                   joinedResult.ProjectId == projectId && 
                   joinedResult.VersionId== versionId).SingleOrDefaultAsync();

               
               if (foundItem == null)
                   throw new Exception("Project not associated with version");



               DataTitleVersionDeployment newDeployment = new DataTitleVersionDeployment
                {
                    Alias = deploymentRequest.Alias,

                    Client = deploymentRequest.ClientType,

                    ClientIdentifier = deploymentRequest.ClientId,

                    IsDeleted = false,

                    VersionId = versionId,

                    PublishDate = DateTime.UtcNow
                };


                var addResult = await userContext.TitleVersionDeployments.AddAsync(newDeployment);

                await userContext.SaveChangesAsync();

                retDeployment = addResult.Entity.ToVersionDeployment(projectId);


                TitleVersion titleVer = new TitleVersion
                {
                    TitleId = retDeployment.ProjectId,
                    VersionId = retDeployment.VersionId,
                    Alias = retDeployment.Alias,
                    LogFullClientMessages = foundItem.LogFullClientMessages,
                    DeploymentId = retDeployment.Id,
                    Version = foundItem.Version,
                    ShortName = foundItem.ProjectName
                };

                await TitleCacheRep.SetAppMappingAsync(deploymentRequest.ClientType, deploymentRequest.ClientId, titleVer);


                _logger.LogInformation($"Adding new deployment ");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding new deployment for projectId {projectId} and version {versionId}");
                throw;
            }


            return retDeployment;

        }

        public async Task RemoveVersionDeploymentAsync(Guid projectId, Guid versionId, Guid deploymentId)
        {
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }

            if (deploymentId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(deploymentId)}");
            }

            try
            {
                await using var userContext = await UserContextRetriever.GetUserDataContextAsync();


                var foundItem = await (from tvd in userContext.TitleVersionDeployments
                    join tv in userContext.TitleVersions on tvd.VersionId equals tv.Id
                    join t in userContext.Titles on tv.TitleId equals t.Id
                    where tvd.Id.Value == deploymentId && tv.Id == versionId && t.Id == projectId
                    select new
                    {
                        ProjectId = t.Id,
                        VersionId = tv.Id,
                        Deployment = tvd,
                        IsVersionDeleted = tv.IsDeleted,
                        t.ShortName,


                    }).SingleOrDefaultAsync();

                if (foundItem == null)
                    throw new Exception("Project and Version not associated with deployment");
                var deploymentItem = foundItem.Deployment;

                if (!deploymentItem.IsDeleted)
                {
                   

                    userContext.TitleVersionDeployments.Update(deploymentItem);
                    deploymentItem.IsDeleted = true;
                    await userContext.SaveChangesAsync();

                }

                await TitleCacheRep.RemoveAppMappingAsync(deploymentItem.Client, deploymentItem.ClientIdentifier, deploymentItem.Alias,
                    false);


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding new deployment for projectId {projectId} and version {versionId}");
                throw;
            }


        }

        public async Task<IEnumerable<AudioFileInfo>> GetAudioFileInfosAsync(Guid projectId, Guid versionId)
        {
            if (projectId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(projectId)}");
            }

            if (versionId == default)
            {
                throw new ArgumentException($"Invalid value provided for {nameof(versionId)}");
            }
                    
            IEnumerable<AudioFileInfo> retAudioFiles = new List<AudioFileInfo>();

            return await Task.FromResult(retAudioFiles);
        }

        private string FormatPhoneNumber(string clientId)
        {
            // remove any dashes, periods, and slashes.
            string retText = clientId;

            var charsToRemove = new string[] { ".", " ", "-", "\\", "/" };
            foreach (var c in charsToRemove)
            {
                retText = retText.Replace(c, string.Empty);
            }


            return retText;
        }

        private bool ValidateSmsClientId(string clientId)
        {
            string internationalPhoneNumber =
                @"\+(9[976]\d|8[987530]\d|6[987]\d|5[90]\d|42\d|3[875]\d|2[98654321]\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)\d{1,14}$";

            return Regex.IsMatch(clientId, internationalPhoneNumber);
        }

        private bool ValidateAlexaClientId(string clientId)
        {
            // Ensure the client id is a valid alexa skill id.
            // amzn1.ask.skill.92304d4d-42a5-4371-9b13-97b4a79b9ad0
            bool isValid = clientId.StartsWith(ALEXASKILLID_PREFIX);


            if (!isValid) return false;
            // check if the later portion of the skill id is a GUID

            string guidPortion = clientId.Substring(ALEXASKILLID_PREFIX.Length, clientId.Length - ALEXASKILLID_PREFIX.Length);
            isValid = Guid.TryParse(guidPortion, out _);



            return isValid;
        }


    }
}
