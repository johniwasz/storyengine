using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    [Produces("application/json")]
    [Route("api/ping")]
    public class PingController : Controller
    {

        [AllowAnonymous]
        [HttpGet()]
        public IActionResult Get()
        {
            return Ok();
        }
    }
}