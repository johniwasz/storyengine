using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.CoreApi.ModelBinders;
using Whetstone.StoryEngine.CoreApi.Models;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Google.Management;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Models.Story;
using Whetstone.StoryEngine.Repository;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [ApiController]
    //  [EnableCors("CorsPolicy")]
    [Route("api/story")]
    public class StoryController : ControllerBase
    {

        private readonly ITitleRepository _titleRep;
        private readonly ITitleReader _titleReader;
        private readonly ILogger<StoryController> _logger;
        private readonly IStoryVersionRepository _storyVersionRep;
        private readonly ITitleValidator _titleValidator;
        private readonly IAlexaIntentExporter _alexaExporter;
        private readonly IDialogFlowManager _dialogFlowManager;

        public StoryController(ITitleReader titleReader, ITitleRepository titleRep, IStoryVersionRepository storyVersionRep,
             ITitleValidator titleValidator, IAlexaIntentExporter alexaExporter, IDialogFlowManager dialogFlowManager, ILogger<StoryController> logger)
        {
            _titleReader = titleReader ?? throw new ArgumentNullException(nameof(titleReader));
            _titleRep = titleRep ?? throw new ArgumentNullException(nameof(titleRep));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _storyVersionRep = storyVersionRep ?? throw new ArgumentNullException(nameof(storyVersionRep));
            _titleValidator = titleValidator ?? throw new ArgumentNullException(nameof(titleValidator));
            _alexaExporter = alexaExporter ?? throw new ArgumentNullException(nameof(alexaExporter));
            _dialogFlowManager = dialogFlowManager ?? throw new ArgumentNullException(nameof(dialogFlowManager));
        }




        [HttpGet()]
        public async Task<IActionResult> GetTitles()
        {

            List<TitleRoot> allTitles = await _titleRep.GetAllTitleDeploymentsAsync();

            return new OkObjectResult(allTitles);

        }


        //[HttpGet("{shortName}")]
        //public async Task<IActionResult> GetTitle(string shortName)
        //{
        //    var story = await _titleRep.GetByShortNameAsync(shortName);
        //    return new OkObjectResult(story.ToStory());

        //}


        [HttpGet("{titleid}/{version}/configuration")]
        public async Task<IActionResult> GetVersionConfiguration([FromRoute(Name = "titleid")] string titleId, [FromRoute(Name = "version")] string version)
        {

            if (string.IsNullOrWhiteSpace(titleId))
                throw new ArgumentNullException(nameof(titleId));

            if (string.IsNullOrWhiteSpace(version))
                throw new ArgumentNullException(nameof(version));


            TitleVersionConfiguration dataVersion = await _storyVersionRep.GetVersionConfigurationAsync(titleId, version);


            return new OkObjectResult(dataVersion);

        }



        [HttpPut("version/{versionId}/configuration")]
        public async Task<IActionResult> PutVersionConfiguration([FromRoute(Name = "versionId")] Guid versionId, [FromBody] UpdateTitleVersionConfigurationRequest updateVersionConfigReq)
        {

            if (versionId == default(Guid))
                throw new ArgumentNullException(nameof(versionId));

            if (updateVersionConfigReq == null)
                throw new ArgumentNullException(nameof(updateVersionConfigReq));


            await _storyVersionRep.UpdateVersionConfigurationAsync(versionId, updateVersionConfigReq);



            return new OkResult();
        }


        //UpdateTitleVersionConfigurationRequest



        [HttpGet("{titleid}/{version}")]
        public async Task<IActionResult> GetVersion([FromRoute(Name = "titleid")] string titleId, [FromRoute(Name = "version")] string version)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;
            StoryTitle title = null;

            List<string> errList = new List<string>();

            if (string.IsNullOrWhiteSpace(titleId))
            {
                statusCode = HttpStatusCode.BadRequest;

                errList.Add($"{nameof(titleId)} cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(version))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(version)} cannot be null or empty");
            }

            if (!errList.Any())
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);


                try
                {
                    title = await _titleReader.GetByIdAsync(titleVer);
                    statusCode = HttpStatusCode.OK;
                }
                catch (TitleNotFoundException)
                {
                    string errMsg = $"Title {titleId} and version {version} not found";
                    errList.Add($"Title {titleId} and version {version} not found");
                    _logger.LogError(errMsg);
                    statusCode = HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    string errMsg = $"Internal error getting title {titleId} and version {version}";
                    errList.Add(errMsg);
                    _logger.LogError(string.Concat(errMsg, $": {ex.ToString()}"));
                    statusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (errList.Any())
            {
                return new JsonHttpStatusResult(errList, statusCode);
            }
            else

                return new OkObjectResult(title);

        }


        [HttpGet("{titleid}/{version}/export")]
        public async Task<IActionResult> ExportVersion([FromRoute(Name = "titleid")] string titleId,
            [FromRoute(Name = "version")] string version)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;

            List<string> errList = new List<string>();
            byte[] titleContents = null;

            if (string.IsNullOrWhiteSpace(titleId))
            {
                statusCode = HttpStatusCode.BadRequest;

                errList.Add($"{nameof(titleId)} cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(version)} cannot be null or empty");
            }

            if (!errList.Any())
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);


                try
                {
                    titleContents = await _storyVersionRep.ExportToZip(titleVer);
                    statusCode = HttpStatusCode.OK;
                }
                catch (TitleNotFoundException)
                {
                    string errMsg = $"Title {titleId} and version {version} not found";
                    errList.Add($"Title {titleId} and version {version} not found");
                    _logger.LogError(errMsg);
                    statusCode = HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    string errMsg = $"Internal error getting title {titleId} and version {version}";
                    errList.Add(errMsg);
                    _logger.LogError(string.Concat(errMsg, $": {ex.ToString()}"));
                    statusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (errList.Any())
            {
                return new JsonHttpStatusResult(errList, statusCode);
            }

            //string fileName = $"{titleId}.{version}.zip";
            //return File(titleContents, "application/zip", fileName);
            return File(titleContents, "application/zip");

        }


        [HttpGet("{titleid}/{version}/dialogflow/export")]
        public async Task<IActionResult> ExportDialogFlow([FromRoute(Name = "titleid")] string titleId, [FromRoute(Name = "version")] string version)
        {
            HttpStatusCode statusCode = HttpStatusCode.OK;

            List<string> errList = new List<string>();
            byte[] titleContents = null;

            if (string.IsNullOrWhiteSpace(titleId))
            {
                statusCode = HttpStatusCode.BadRequest;

                errList.Add($"{nameof(titleId)} cannot be null or empty");
            }
            if (string.IsNullOrWhiteSpace(version))
            {
                statusCode = HttpStatusCode.BadRequest;
                errList.Add($"{nameof(version)} cannot be null or empty");
            }

            if (!errList.Any())
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);


                try
                {

                    StoryTitle foundTitle = await _titleReader.GetByIdAsync(titleVer);
                    titleContents = await _dialogFlowManager.ExportTitleNlpAsync(foundTitle, null);
                    statusCode = HttpStatusCode.OK;
                }
                catch (TitleNotFoundException)
                {
                    string errMsg = $"Title {titleId} and version {version} not found";
                    errList.Add($"Title {titleId} and version {version} not found");
                    _logger.LogError(errMsg);
                    statusCode = HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    string errMsg = $"Internal error getting title {titleId} and version {version}";
                    errList.Add(errMsg);
                    _logger.LogError(string.Concat(errMsg, $": {ex.ToString()}"));
                    statusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (errList.Any())
            {
                return new JsonHttpStatusResult(errList, statusCode);
            }
            else
            {
                string fileName = $"{titleId}.{version}-dialogflow.zip";
                return File(titleContents, "application/zip", fileName);
            }
        }


        [HttpPost("clone")]
#pragma warning disable SCS0016
        public async Task<IActionResult> CloneVersion([ModelBinder(typeof(LamdaSubmissionBinder<CloneRequest>))] CloneRequest cloneReq)
#pragma warning restore SCS0016
        {

            HttpStatusCode statusCode = HttpStatusCode.OK;
            List<string> errList = new List<string>();

            string titleId = null;
            string sourceVersion = null;
            string destVersion = null;
            StoryTitle retTitle = null;
            if (cloneReq == null)
            {
                errList.Add($"{nameof(cloneReq)} cannot be null");
                statusCode = HttpStatusCode.BadRequest;
            }
            else
            {
                titleId = cloneReq.TitleId;
                sourceVersion = cloneReq.SourceVersion;
                destVersion = cloneReq.DestinationVersion;

                if (string.IsNullOrWhiteSpace(titleId))
                {
                    statusCode = HttpStatusCode.BadRequest;

                    errList.Add($"TitleId in CloneRequest cannot be null or empty");
                }
                if (string.IsNullOrWhiteSpace(sourceVersion))
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errList.Add($"SourceVersion in CloneRequest cannot be null or empty");
                }
                if (string.IsNullOrWhiteSpace(destVersion))
                {
                    statusCode = HttpStatusCode.BadRequest;
                    errList.Add($"DestinationVersion in CloneRequest cannot be null or empty");
                }

            }


            if (!errList.Any())
            {
                try
                {
                    retTitle = await _storyVersionRep.CloneVersionAsync(titleId, sourceVersion, destVersion);
                    statusCode = HttpStatusCode.OK;
                }
                catch (TitleNotFoundException ex)
                {
                    string errMsg = $"Title {titleId} and version {sourceVersion} not found";
                    errList.Add($"Title {titleId} and version {sourceVersion} not found");
                    _logger.LogError(string.Concat(errMsg, $": {ex.ToString()}"));
                    statusCode = HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    string errMsg = $"Internal error cloning from title {titleId} and version {sourceVersion} to {destVersion}";
                    errList.Add(errMsg);
                    _logger.LogError(string.Concat(errMsg, $": {ex.ToString()}"));
                    statusCode = HttpStatusCode.InternalServerError;
                }
            }

            if (errList.Any())
                return new JsonHttpStatusResult(errList, statusCode);


            return new OkObjectResult(retTitle);

        }


        [HttpPost("")]
#pragma warning disable SCS0016
        public async Task<IActionResult> UpdateVersion([ModelBinder(typeof(LamdaSubmissionBinder<StoryTitle>))] StoryTitle storyTitle)
#pragma warning restore SCS0016
        {
            List<string> errList = new List<string>();
            HttpStatusCode statCode = HttpStatusCode.OK;
            if (storyTitle == null)
            {
                errList.Add($"{nameof(storyTitle)} cannot be null. Submission deserialization failed.");
                statCode = HttpStatusCode.BadRequest;

            }
            if (!errList.Any())
            {
                try
                {
                    await _storyVersionRep.CreateOrUpdateVersionAsync(storyTitle);
                    statCode = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {

                    statCode = HttpStatusCode.InternalServerError;
                    errList.Add("Internal error updating title version");
                    _logger.LogError($"Error updating version: {ex.ToString()}");

                }
            }


            if (errList.Any())
            {
                return new JsonHttpStatusResult(errList, statCode);
            }
            else
            {
                return new OkResult();
            }
        }


        [HttpPost("publish")]
#pragma warning disable SCS0016
        public async Task<IActionResult> Publish([ModelBinder(typeof(LamdaSubmissionBinder<PublishVersionRequest>))] PublishVersionRequest publishRequest)
#pragma warning restore SCS0016
        {
            List<string> errResult = new List<string>();

            if (publishRequest == null)
                errResult.Add($"{nameof(publishRequest)} cannot be null");

            if (string.IsNullOrWhiteSpace(publishRequest?.ClientId))
                errResult.Add($"{nameof(publishRequest)} ClientId cannot be null or empty");

            if (string.IsNullOrWhiteSpace(publishRequest?.TitleName))
                errResult.Add($"{nameof(publishRequest)} TitleName cannot be null or empty");

            if (string.IsNullOrWhiteSpace(publishRequest?.Version))
                errResult.Add($"{nameof(publishRequest)} Version cannot be null or empty");


            if (!errResult.Any())
            {
                try
                {
                    await _storyVersionRep.PublishVersionAsync(publishRequest);
                }
                catch (Exception ex)
                {
                    errResult.Add("Internal error processing publish request");
                    _logger.LogError(ex, $"Error attempting to publish short name {publishRequest.TitleName} and version {publishRequest.Version} for client {publishRequest.ClientType} and clientId {publishRequest.ClientId}");

                }
            }


            if (errResult.Any())
            {
                return new OkObjectResult(errResult);
            }
            else
                return new OkResult();
        }


        [HttpGet("{titleid}/{version}/alexaintents")]
        [HttpGet("{titleid}/{version}/alexaintents/{locale}")]
        public async Task<IActionResult> GetAlexaIntents([FromRoute(Name = "titleid")] string titleId,
                                                         [FromRoute(Name = "version")] string version,
                                                         [FromRoute(Name = "locale")] string locale = "en-US")
        {
            List<string> errResults = new List<string>();
            HttpStatusCode statusCode;
            JsonResult interactionResult = null;

            if (string.IsNullOrWhiteSpace(titleId))
            {
                errResults.Add("titleId cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                errResults.Add("version cannot be null or empty");
            }

            try
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);

                var title = await _titleRep.GetByIdAsync(titleVer);
                if (title != null)
                {
                    // var globalConfig =await  S3Client.GetConfigFileContentsAsync();

                    var alexaIntents = await _alexaExporter.GetIntentModelAsync(title, locale);


                    JsonSerializerSettings serSettings = new JsonSerializerSettings
                    {
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };

                    // serSettings.TypeNameHandling = TypeNameHandling.Objects;

                    serSettings.Converters.Add(new StringEnumConverter
                    {
                        NamingStrategy = new CamelCaseNamingStrategy()
                    });


                    interactionResult = new JsonResult(alexaIntents, serSettings);
                    statusCode = HttpStatusCode.OK;
                }
                else
                {
                    string notFound = $"Title {titleId} and version {version} not found";

                    _logger.LogError(notFound);

                    errResults.Add(notFound);
                    statusCode = HttpStatusCode.NotFound;

                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting intents for title {titleId} and version {version}: {ex.ToString()}");
                errResults.Add("Internal error exporting intents");
                statusCode = HttpStatusCode.InternalServerError;

            }


            if (errResults.Any())
            {
                return new JsonHttpStatusResult(errResults, statusCode);
            }
            else
            {
                return interactionResult;
            }
        }

        [HttpGet("{titleid}/{version}/zipexport")]
        public IActionResult ExportZipFile([FromRoute(Name = "titleid")] string titleId,
                                                        [FromRoute(Name = "version")] string version)

        {
            List<string> errResults = new List<string>();
            HttpStatusCode statusCode = HttpStatusCode.OK;
            JsonResult interactionResult = null;

            if (string.IsNullOrWhiteSpace(titleId))
            {
                errResults.Add("titleId cannot be null or empty");
                statusCode = HttpStatusCode.BadRequest;
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                errResults.Add("version cannot be null or empty");
                statusCode = HttpStatusCode.BadRequest;
            }

            //try
            //{
            //    TitleVersion titleVer = new TitleVersion(titleId, version);

            //    var title = await _titleRep.GetByIdAsync(_env, titleVer);
            //    if (title != null)
            //    {
            //        // var globalConfig =await  S3Client.GetConfigFileContentsAsync();

            //        var alexaIntents = await _alexaExporter.GetIntentModelAsync(title, locale);


            //        JsonSerializerSettings serSettings = new JsonSerializerSettings();
            //        serSettings.Formatting = Formatting.Indented;
            //        serSettings.NullValueHandling = NullValueHandling.Ignore;
            //        serSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            //        // serSettings.TypeNameHandling = TypeNameHandling.Objects;

            //        serSettings.Converters.Add(new StringEnumConverter
            //        {
            //            NamingStrategy = new CamelCaseNamingStrategy()
            //        });


            //        interactionResult = new JsonResult(alexaIntents, serSettings);
            //        statusCode = HttpStatusCode.OK;
            //    }
            //    else
            //    {
            //        string notFound = $"Title {titleId} and version {version} not found";

            //        _logger.LogError(notFound);

            //        errResults.Add(notFound);
            //        statusCode = HttpStatusCode.NotFound;

            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError($"Error exporting intents for title {titleId} and version {version}: {ex.ToString()}");
            //    errResults.Add("Internal error exporting intents");
            //    statusCode = HttpStatusCode.InternalServerError;

            //}


            if (errResults.Any())
            {
                return new JsonHttpStatusResult(errResults, statusCode);
            }
            else
            {
                return interactionResult;
            }
        }



        [HttpDelete("{titleid}/{version}")]
#pragma warning disable SCS0016
        public async Task<IActionResult> DeleteVersion([FromRoute(Name = "titleid")] string titleId,
                                                         [FromRoute(Name = "version")] string version)
#pragma warning restore SCS0016
        {
            List<string> errResults = new List<string>();
            HttpStatusCode statusCode;

            if (string.IsNullOrWhiteSpace(titleId))
            {
                errResults.Add("titleId cannot be null or empty");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                errResults.Add("version cannot be null or empty");
            }

            try
            {
                await _storyVersionRep.DeleteVersionAsync(titleId, version);
                statusCode = HttpStatusCode.OK;
            }
            catch (TitleNotFoundException notFoundEx)
            {
                _logger.LogError($"Error deleting title {titleId} and version {version}: {notFoundEx.ToString()}");
                errResults.Add($"Title {titleId} and version {version} not found");
                statusCode = HttpStatusCode.NotFound;
            }
            catch (Exception ex)
            {
                statusCode = HttpStatusCode.InternalServerError;
                _logger.LogError($"Error deleting title {titleId} and version {version}: {ex.ToString()}");
                errResults.Add($"Internal error deleting title {titleId} and version {version}");
            }

            if (errResults.Any())
            {
                return new JsonHttpStatusResult(errResults, statusCode);
            }
            else
            {
                return new OkResult();

            }

        }


        [HttpPost("validate")]
#pragma warning disable SCS0016
        public async Task<IActionResult> Validate([ModelBinder(typeof(LamdaSubmissionBinder<StoryTitle>))] StoryTitle title)
#pragma warning restore SCS0016
        {

            List<string> errResult = new List<string>();
            HttpStatusCode retCode = HttpStatusCode.OK;

            if (title == null)
            {
                errResult.Add($"{nameof(title)} cannot be null");
                retCode = HttpStatusCode.BadRequest;
            }
            StoryValidationResult valResult = null;

            if (!errResult.Any())
            {
                try
                {


                    valResult = await _titleValidator.ValidateTitleAsync(title);

                }
                catch (Exception ex)
                {
                    retCode = HttpStatusCode.InternalServerError;
                    errResult.Add("Internal error processing validation request");
                    _logger.LogError($"Error attempting to validate title: {ex.ToString()}");

                }
            }


            if (errResult.Any())
            {
                return new JsonHttpStatusResult(errResult, retCode);
            }

            return new OkObjectResult(valResult);

        }


    }


}
