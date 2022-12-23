using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Story;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Options;
using Whetstone.StoryEngine.AlexaProcessor;
using Whetstone.StoryEngine.CoreApi.ModelBinders;
using Whetstone.StoryEngine.Models.Configuration;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    /// <summary>
    /// ASP.NET Core controller acting as a S3 Proxy.
    /// </summary>

    // [EnableCors("CorsPolicy")]
    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [Route("api/fileconfig")]
    public class FileConfigController : Controller
    {
        ILogger Logger { get; set; }
        IAlexaIntentExporter IntentExporter { get; set; }
        private readonly ITitleRepository _titleRep = null;

        
        public FileConfigController(
            IAlexaIntentExporter intentExporter, 
            ILogger<FileConfigController> logger,
            ITitleRepository titleRep)
        {
            this.Logger = logger;
          
            this.IntentExporter = intentExporter;
            _titleRep = titleRep;


        }

        [Produces("application/json")]
        [HttpGet("{titleId}/alexaintents")]
        public async Task<IActionResult> ExportAlexaIntents(string titleId, string version,  string locale = "en-US")
        {


            try
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);

                var title = await _titleRep.GetByIdAsync(titleVer);
                if (title != null)
                {
                   // var globalConfig =await  S3Client.GetConfigFileContentsAsync();

                    var alexaIntents = await IntentExporter.GetIntentModelAsync(title, locale);


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


                    return new JsonResult(alexaIntents, serSettings);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error exporting intents for title {titleId}");
                return new StatusCodeResult(500);
            }


        }
        [HttpGet("{titleId}")]
        public async Task<IActionResult> Get(string titleId, string version)
        {
            try
            {
                TitleVersion titleVer = new TitleVersion(titleId, version);

                StoryTitle title = await _titleRep.GetByIdAsync(titleVer);
                return new ObjectResult(title);
               // return new JsonResult(title);
            } 
            catch(Exception ex)
            {
                Logger.LogError(ex, "Error getting title {0}", titleId);
                return new StatusCodeResult(500);
            }
        }
        
        [Route("")]
        [HttpPost()]
        [HttpPost(".{format}"), FormatFilter]
#pragma warning disable SCS0016
        public async Task<IActionResult> Post([ModelBinder(typeof(LamdaSubmissionBinder<StoryTitle>))] StoryTitle title)
#pragma warning restore SCS0016
        {
            if (title == null)
            {
                Logger.LogError("Title null. Most likely cause is a deserialization failure.");
                return new BadRequestResult();
               
            }


            if(string.IsNullOrWhiteSpace(title.Id))
            {
                Logger.LogError("Error storing title: titleId is null or empty");
                return new BadRequestResult();
            }


            try
            {
                await _titleRep.CreateOrUpdateTitleAsync(title);
                // Update client in cache.



                return new OkResult();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error storing title {0}", title.Id);
                return new StatusCodeResult(500);
            }



        }





    }
}
