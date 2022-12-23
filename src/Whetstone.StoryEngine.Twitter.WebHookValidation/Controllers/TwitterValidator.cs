using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Data;
using Whetstone.StoryEngine.Models.Twitter;

namespace Whetstone.StoryEngine.Twitter.WebHookValidation.Controllers
{

    [Route("api/twitter")]
    public class TwitterValidator : ControllerBase
    {
        private ITwitterValidator _twitterValidator;


        public TwitterValidator(ITwitterValidator twitterValidator)
        {
            _twitterValidator = twitterValidator ?? throw new ArgumentNullException(nameof(twitterValidator));

        }

        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery(Name = "credid")] Guid credid, [FromQuery(Name ="orgid")] Guid organizationid, [FromQuery] string nonce, [FromQuery(Name = "crc_token")] string crcToken)
        {
           TwitterCrcResponse crcResp = await _twitterValidator.GenerateTwitterCrcResponseAsync(organizationid, credid, crcToken);

            return new OkObjectResult(crcResp);
        }

    }
}
