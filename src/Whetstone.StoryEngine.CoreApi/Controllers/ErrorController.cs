using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

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
