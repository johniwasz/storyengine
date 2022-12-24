using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Whetstone.StoryEngine.Models.Admin;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Whetstone.StoryEngine.CoreApi.Controllers
{
    [Authorize(Security.FunctionalEntitlements.IsRegisteredUser)]
    [ApiController]
    [Route("api/uniqueid")]
    public class UniqueIdController : Controller
    {
        [HttpGet()]
        public UniqueId Get()
        {
            return new UniqueId
            {
                Id = Guid.NewGuid().ToString()
            };
        }
    }
}
