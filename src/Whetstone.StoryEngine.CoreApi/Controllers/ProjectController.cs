using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Whetstone.StoryEngine.CoreApi.Models;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Notifications;
using Whetstone.StoryEngine.Notifications.Repository;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{


    [ApiController]
    //  [EnableCors("CorsPolicy")]
    [Route("api/project")]
    public class ProjectController : ControllerBase
    {

        private readonly IProjectRepository _projectRepository;
        //private readonly ILogger<StoryController> _logger;

        //private readonly ITitleValidator _titleValidator;
        //private readonly IAlexaIntentExporter _alexaExporter;
        //private readonly IDialogFlowManager _dialogFlowManager;
        private readonly IFileRepository _fileRepo;

        private readonly INotificationProcessor _notificationProcessor;

        public ProjectController(IProjectRepository projectRepository, IFileRepository fileRepo, INotificationProcessor notificationProcessor)
        {
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //_titleValidator = titleValidator ?? throw new ArgumentNullException(nameof(titleValidator));
            //_alexaExporter = alexaExporter ?? throw new ArgumentNullException(nameof(alexaExporter));
            //_dialogFlowManager = dialogFlowManager ?? throw new ArgumentNullException(nameof(dialogFlowManager));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));

            _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));

            _notificationProcessor = notificationProcessor;
        }



        [Authorize(Security.FunctionalEntitlements.PermissionViewProject)]
        [HttpGet()]
        public async Task<IActionResult> GetProjects()
        {
            IEnumerable<Project> allProjects = await _projectRepository.GetProjectsAsync();
            return new OkObjectResult(allProjects);
        }

        [Authorize(Security.FunctionalEntitlements.PermissionViewVersion)]
        [Route("{id}/version")]
        [HttpGet]
        public async Task<IActionResult> GetVersions([FromRoute] Guid id)
        {
            IEnumerable<ProjectVersion> projectVersions = await _projectRepository.GetVersionsAsync(id);
            return new OkObjectResult(projectVersions);
        }


        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}")]
        [HttpPut]
        public async Task<IActionResult> UpdateVersion([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromBody] ProjectVersionUpdateRequest updateRequest)
        {
            ProjectVersion updatedVersion = await _projectRepository.UpdateVersionAsync(projectId, versionId, updateRequest);
            return new OkObjectResult(updatedVersion);
        }


        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/deployment")]
        [HttpGet]
        public async Task<IActionResult> GetVersionDeployments([FromRoute] Guid projectId, [FromRoute] Guid versionId)
        {
            IEnumerable<VersionDeployment> deployments = await _projectRepository.GetVersionDeploymentsAsync(projectId, versionId);
            return new OkObjectResult(deployments);
        }


        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/audio")]
        [HttpGet]
        public async Task<IActionResult> GetAudioFiles([FromRoute] Guid projectId, [FromRoute] Guid versionId)
        {
            IEnumerable<AudioFileInfo> audioFiles = await _fileRepo.GetAudioFileListAsync(projectId, versionId);
            return new OkObjectResult(audioFiles);
        }

        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/audio/{fileName}")]
        [HttpGet]
        public async Task<IActionResult> GetAudioInfo([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromRoute] string fileName)
        {
            AudioFileInfo fileInfo = await _fileRepo.GetAudioFileInfoAsync(projectId, versionId, fileName);
            return new OkObjectResult(fileInfo);
        }


        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/audio/{fileName}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteAudioFile([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromRoute] string fileName)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;

            List<string> errList = new List<string>();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                statusCode = HttpStatusCode.BadRequest;

                errList.Add($"{nameof(fileName)} cannot be null or empty");
            }
            if (projectId == default(Guid))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(projectId)} must have a value");
            }
            if (versionId == default(Guid))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(versionId)} must have a value");
            }

            if (!errList.Any())
            {
                await _fileRepo.DeleteAudioFileAsync(projectId, versionId, fileName);
                return new OkResult();
            }
            else
            {
                return new JsonHttpStatusResult(errList, statusCode);
            }
        }

        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/audio/{fileName}/content")]
        [HttpGet]
        public async Task<IActionResult> GetAudioContent([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromRoute] string fileName)
        {
            FileContentStream fileContents = await _fileRepo.GetFileContentStreamAsync(projectId, versionId, fileName);
            return File(fileContents.Content, fileContents.MimeType, fileName);
        }


        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/audio/{fileName}/content")]
        [HttpPost]
        public async Task<IActionResult> AddAudioContent([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromRoute] string fileName, [FromForm] SimpleFileUpload formData)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;

            List<string> errList = new List<string>();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                statusCode = HttpStatusCode.BadRequest;

                errList.Add($"{nameof(fileName)} cannot be null or empty");
            }
            if (projectId == default(Guid))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(projectId)} must have a value");
            }
            if (versionId == default(Guid))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(versionId)} must have a value");
            }
            if (formData == null)
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(formData)} cannot be null");
            }
            else
            {
                if (formData.UploadedFile == null)
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errList.Add($"{nameof(formData.UploadedFile)} cannot be null");
                }
            }

            if (!errList.Any())
            {
                // Use the stream that's passed in, if it's <=64K it will be in memory, otherwise it will be to a file on disk - this
                // should keep us from tanking memory. We should have some sort of a max file size we enforce.
                using (Stream stm = formData.UploadedFile.OpenReadStream())
                {
                    // If we were passed a client id, then the result should come back asynchronously
                    if (String.IsNullOrEmpty(formData.ClientId))
                    {
                        AudioFileInfo fileInfo = await _fileRepo.StoreAudioFileAsync(projectId, versionId, fileName, stm);
                        return new OkObjectResult(fileInfo);
                    }
                    else
                    {

                        UploadMediaFileInfo uploadInfo = await _fileRepo.UploadAudioFileAsync(projectId, versionId, fileName, stm);

                        if (uploadInfo != null)
                        {
                            AudioFileInfo fileInfo = await _fileRepo.CommitUploadedAudioFileAsync(uploadInfo);

                            string userId = null;
                            foreach (Claim cl in this.User.Claims)
                            {
                                if (cl.Type == "username")
                                {
                                    userId = cl.Value;
                                }
                            }

                            NotificationRequest notificationReq = new NotificationRequest
                            {
                                UserId = userId,
                                NotificationId = Guid.NewGuid().ToString(),
                                ClientId = formData.ClientId,
                                RequestType = NotificationRequestType.SendNotificationToClient,
                                NotificationType = NotificationDataType.AddAudioFileToProject,
                                Data = JsonConvert.SerializeObject(fileInfo)
                            };

                            // this is a fire and forget async call
                            await _notificationProcessor.ProcessNotification(notificationReq);

                            AsyncNotification asyncNotification = new AsyncNotification
                            {
                                NotificationId = notificationReq.NotificationId
                            };

                            return new OkObjectResult(asyncNotification);
                        }

                        return new EmptyResult();

                    }
                }
            }
            else
            {
                return new JsonHttpStatusResult(errList, statusCode);
            }


        }

        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/deployment/{deploymentId}")]
        [HttpDelete]
        public async Task<IActionResult> DeleteVersionDeployment([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromRoute] Guid deploymentId)
        {
            await _projectRepository.RemoveVersionDeploymentAsync(projectId, versionId, deploymentId);
            return new OkResult();
        }

        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("{projectId}/version/{versionId}/deployment")]
        [HttpPost]
        public async Task<IActionResult> AddDeployment([FromRoute] Guid projectId, [FromRoute] Guid versionId, [FromBody] AddVersionDeploymentRequest deploymentRequest)
        {
            VersionDeployment deploymentResult = await _projectRepository.AddVersionDeploymentAsync(projectId, versionId, deploymentRequest);
            return new OkObjectResult(deploymentResult);
        }

    }
}
