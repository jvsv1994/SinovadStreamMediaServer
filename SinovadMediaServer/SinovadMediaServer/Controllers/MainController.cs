using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Shared;

namespace SinovadMediaServer.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {

        public MiddlewareInjectorOptions _middlewareInjectorOptions;

        public MainController(MiddlewareInjectorOptions middlewareInjectorOptions)
        {
            _middlewareInjectorOptions = middlewareInjectorOptions;
        }

        [HttpPost("StartApplication")]
        public ActionResult StartApplication()
        {
            return Ok();
        }

    }
}
