using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Whetstone.StoryEngine.CoreApi.Models;
using Whetstone.StoryEngine.Models.Admin;
using Whetstone.StoryEngine.Security;


namespace Whetstone.StoryEngine.CoreApi.Controllers
{

    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [Route("api/token")]
    [ApiController]
    public sealed class TokensController : ControllerBase
    {
        private readonly IAuthenticator _authenticator;
        private readonly ILogger<TokensController> _logger;

        private const string AuthorizationHeaderName = "Authorization";



        public TokensController(
            IAuthenticator authenticator,
            ILogger<TokensController> logger)
        {
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AuthenticateAsync(AuthCredentials creds)
        {
            if (creds == null)
                ModelState.AddModelError("BadMessage", "Authentication credentials cannot be null");
            else
            {
                if (string.IsNullOrWhiteSpace(creds.UserName))
                    ModelState.AddModelError("BadMessage", "name is required");

                if (string.IsNullOrWhiteSpace(creds.UserSecret))
                    ModelState.AddModelError("BadMessage", "password is required");
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            TokenResult tokenRes;


            tokenRes = await _authenticator.AuthenticateAsync(creds);


            return new OkObjectResult(tokenRes);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("refresh")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequest refreshRequest)
        {   // GetTokensAsync is a bust
            // string accessToken = await HttpContext.GetTokenAsync(JwtBearerDefaults.AuthenticationScheme, "access_token");
            string accessToken = null;
            if (Request.Headers.ContainsKey(AuthorizationHeaderName))
            {
                var authHeader = Request.Headers[AuthorizationHeaderName];

                if (authHeader.Count == 1)
                {
                    string authHeaderVal = authHeader[0];
                    string[] headerVals = authHeaderVal.Split(' ');
                    if (headerVals.Length == 2)
                    {
                        if (headerVals[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                        {
                            accessToken = headerVals[1];
                        }
                    }
                }
            }


            if (refreshRequest == null)
                ModelState.AddModelError("BadMessage", "refreshToken cannot be null");
            else
            {
                if (string.IsNullOrWhiteSpace(refreshRequest.RefreshToken))
                    ModelState.AddModelError("BadMessage", "refreshToken is required");
            }

            if (string.IsNullOrWhiteSpace(accessToken))
                ModelState.AddModelError("BadMessage", "access_token is required in header");


            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            TokenResult tokRes = null;

            try
            {
                RefreshSessionRequest refSession = new RefreshSessionRequest
                {

                    // ReSharper disable once PossibleNullReferenceException
                    RefreshToken = refreshRequest.RefreshToken,
                    AuthToken = accessToken
                };

                tokRes = await _authenticator.RefreshTokenAsync(refSession);

            }
            catch (UnexpectedSecurityException adminEx)
            {
                _logger.LogError("Error refreshing token", adminEx);
                ModelState.AddModelError("RefreshTokenError", adminEx.PublicMessage);
                return BadRequest(ModelState);


            }
            catch (AdminException ex)
            {
                _logger.LogError("Error refreshing token", ex);
                ModelState.AddModelError("RefreshTokenError", ex.PublicMessage);
                return BadRequest(ModelState);
            }


            return new OkObjectResult(tokRes);
        }

        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [HttpPost]
        [Route("signout")]
        public async Task<IActionResult> SignOutAsync(SignOutRequest request)
        {

            await _authenticator.SignOutAsync(request);

            return this.Ok();
        }


    }
}
