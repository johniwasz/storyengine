using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Whetstone.StoryEngine.Models.Admin;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    [ApiController]
    public class ErrorController : ControllerBase
    {

        public ErrorController()
        {

        }


        [Route("/error")]
        public IActionResult Error(
            [FromServices] IWebHostEnvironment webHostEnvironment)
        {
            return Problem();
        }


    }
}
