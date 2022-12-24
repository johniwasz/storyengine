using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Whetstone.StoryEngine.Security;
using Whetstone.StoryEngine.Security.Claims;
using SignUpRequest = Whetstone.StoryEngine.Security.SignUpRequest;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{



    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        private readonly IAuthenticator _authenticator;

        public UserController(
            IAuthenticator authenticator,
            ILogger<UserController> logger)
        {
            _authenticator = authenticator ?? throw new ArgumentNullException(nameof(authenticator));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUpAsync(SignUpRequest signUpRequest)
        {
            if (signUpRequest == null)
                ModelState.AddModelError("BadMessage", "Sign up request cannot be null");
            else
            {
                if (string.IsNullOrWhiteSpace(signUpRequest.UserName))
                    ModelState.AddModelError("BadMessage", "name is required");

                if (string.IsNullOrWhiteSpace(signUpRequest.UserSecret))
                    ModelState.AddModelError("BadMessage", "password is required");

                if (!signUpRequest.AreTermsAccepted)
                    ModelState.AddModelError("BadMessage", "user must accept terms");

            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            await _authenticator.SignUpAsync(signUpRequest);

            return new AcceptedResult();
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("confirm")]
        public async Task<IActionResult> ConfirmUserAsync(ConfirmNewUserRequest confirmUser)
        {
            if (confirmUser == null)
                ModelState.AddModelError("BadMessage", "Sign up request cannot be null");
            else
            {
                if (string.IsNullOrWhiteSpace(confirmUser.UserName))
                    ModelState.AddModelError("BadMessage", "name is required");

                if (string.IsNullOrWhiteSpace(confirmUser.ConfirmationCode))
                    ModelState.AddModelError("BadMessage", "confirmationCode is required");
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            ConfirmNewUserResponse confirmRep = await _authenticator.ConfirmAccountAsync(confirmUser);

            return new OkObjectResult(confirmRep);
        }

        /// <summary>
        /// Retrieves the set of permissions of the user currently authenticated.
        /// </summary>
        /// <remarks>
        /// The permissions returned should be used to determine which user interface elements to hide or display.
        /// </remarks>
        /// <returns>An array of permissions.</returns>
        [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
        [Route("permissions")]
        public async Task<IActionResult> GetPermissionsAsync()
        {

            return await Task.Run(() =>
             {

                 var perms = this.User.Claims.Where(x => x.Type.Equals(SoniBridgeClaimTypes.Permission))
                     .Select(x => x.Value);


                 return new OkObjectResult(perms);
             });
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("newconfirmcode")]
        public async Task<IActionResult> RequestNewConfirmationCodeAsync(
            ConfirmationCodeResendRequest newConfirmationCodeRequest)
        {
            if (newConfirmationCodeRequest == null)
                ModelState.AddModelError("BadMessage", "newConfimationCodeRequest cannot be null");
            else
            {
                if (string.IsNullOrWhiteSpace(newConfirmationCodeRequest.UserName))
                    ModelState.AddModelError("BadMessage", "name is required");
            }

            if (ModelState.ErrorCount > 0)
                return BadRequest(ModelState);

            await _authenticator.RequestNewConfirmationCode(newConfirmationCodeRequest);

            return new AcceptedResult();
        }

    }
}
