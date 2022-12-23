using Google.Rpc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Whetstone.StoryEngine.CoreApi.Models;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Admin;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [ApiController]
    //  [EnableCors("CorsPolicy")]
    [Route("api/organization")]
    public class OrganizationController : ControllerBase
    {
        private IOrganizationRepository _orgRepository;
        public OrganizationController(IOrganizationRepository orgRepository)
        {
            //_logger = logger ?? throw new ArgumentNullException(nameof(logger));

            //_titleValidator = titleValidator ?? throw new ArgumentNullException(nameof(titleValidator));
            //_alexaExporter = alexaExporter ?? throw new ArgumentNullException(nameof(alexaExporter));
            //_dialogFlowManager = dialogFlowManager ?? throw new ArgumentNullException(nameof(dialogFlowManager));
            _orgRepository = orgRepository ?? throw new ArgumentNullException(nameof(orgRepository));

            // _fileRepo = fileRepo ?? throw new ArgumentNullException(nameof(fileRepo));

            // _notificationProcessor = notificationProcessor;
        }




        [HttpGet()]
        public async Task<IActionResult> GetOrganizations()
        {

            Thread.CurrentPrincipal = this.User;
            IEnumerable<Organization> allProjects = await _orgRepository.GetOrganizationsAsync();
            return new OkObjectResult(allProjects);
        }

        //[Route("{organizationid}/twittercredentials")]
        //[HttpPost()]
        //public async Task<IActionResult> AddTwitterCredentials(Guid organizationId, AddTwitterCredentialsRequest addCredsRequest)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    AddTwitterCredentialsResponse resp = await _orgRepository.AddTwitterCredentialsAsync(organizationId, addCredsRequest);
        //    return new OkObjectResult(resp);
        //}


        //[Route("{organizationid}/twittercredentials")]
        //[HttpGet()]
        //public async Task<IActionResult> GetTwitterCredentials(Guid organizationId)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    var resp = await _orgRepository.GetTwitterCredentialsAsync(organizationId);
        //    return new OkObjectResult(resp);
        //}

        //[Route("{organizationid}/twittercredentials/{credentialid}")]
        //[HttpGet()]
        //public async Task<IActionResult> GetTwitterCredential(Guid organizationId, Guid credentialId)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    var resp = await _orgRepository.GetTwitterCredentialAsync(organizationId, credentialId);
        //    return new OkObjectResult(resp);
        //}

        //[Route("{organizationid}/twittercredentials/{credentialsid}")]
        //[HttpDelete()]
        //public async Task<IActionResult> DeleteTwitterCredentials(Guid organizationId, Guid credentialsId)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    await _orgRepository.DeleteTwitterCredentialsAsync(organizationId, credentialsId);
        //    return Ok();
        //}


        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/webhooks")]
        //[HttpGet()]
        //public async Task<IActionResult> GetTwitterWebhooks(Guid organizationId, Guid credentialId, string environment)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    var resp = await _orgRepository.GetTwitterWebhooksAsync(organizationId, credentialId, environment);
        //    return new OkObjectResult(resp);
        //}


        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/subscriptions")]
        //[HttpGet()]
        //public async Task<IActionResult> GetTwitterSubscriptions(Guid organizationId, Guid credentialId, string environment)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    var resp = await _orgRepository.GetTwitterSubscriptionsAsync(organizationId, credentialId, environment);
        //    return new OkObjectResult(resp);
        //}

        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/subscriptions")]
        //[HttpPost()]
        //public async Task<IActionResult> AddTwitterSubscription(Guid organizationId, Guid credentialId, string environment)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    await _orgRepository.SubscribeAsync(organizationId, credentialId, environment);
        //    return Ok();
        //}

        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/subscriptions/{userId}")]
        //[HttpDelete()]
        //public async Task<IActionResult> TwitterUnsubscribe(Guid organizationId, Guid credentialId, string environment, long userId)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    await _orgRepository.UnsubscribeAsync(organizationId, credentialId, environment, userId);
        //    return Ok();
        //}

        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/webhooks/{webhookid}")]
        //[HttpDelete()]
        //public async Task<IActionResult> DeleteTwitterWebhook(Guid organizationId, Guid credentialId, string environment, string webhookid)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    await _orgRepository.DeleteTwitterWebhookAsync(organizationId, credentialId, environment, webhookid);
        //    return Ok();
        //}



        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/webhooks/{webhookid}/validate")]
        //[HttpPost()]
        //public async Task<IActionResult> ResendTwitterWebhookValidation(Guid organizationId, Guid credentialId, string environment, string webhookid)
        //{
        //    Thread.CurrentPrincipal = this.User;
        //    await _orgRepository.ResendWebhookValidationAsync(organizationId, credentialId, environment, webhookid);
        //    return Ok();
        //}


        //[Route("{organizationid}/twittercredentials/{credentialid}/{environment}/webhooks")]
        //[HttpPost()]
        //public async Task<IActionResult> RegisterTwitterWebhook([FromRoute] Guid organizationid, [FromRoute] Guid credentialid, [FromRoute] string environment, [FromBody] WebHookRegistrationRequest urlModel)
        //{         
        //    if (Uri.TryCreate(urlModel?.Url, UriKind.Absolute, out Uri webHookUrl))
        //    {
        //        Thread.CurrentPrincipal = this.User;
        //        TwitterWebhookListItem webHookItem = await _orgRepository.RegisterTwitterWebhookAsync(organizationid, credentialid, environment, webHookUrl);
        //        return new OkObjectResult(webHookItem);
        //    }
        //    else
        //    {
        //        return new BadRequestResult();
        //    }
        //}

    }
}
