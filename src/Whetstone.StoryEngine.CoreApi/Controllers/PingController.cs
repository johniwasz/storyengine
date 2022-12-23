using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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