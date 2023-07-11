using SinovadMediaServer.CustomModels;
using SinovadMediaServer.Middleware;
using SinovadMediaServer.Shared;
using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Enums;

namespace SinovadMediaServer.Controllers
{
    [Route("[controller]")]
    public class MainController : Controller
    {
        
        private readonly SharedData _sharedData;

        private readonly SharedService _sharedService;

        public MiddlewareInjectorOptions _middlewareInjectorOptions;

        public MainController(SharedService sharedService, SharedData sharedData, MiddlewareInjectorOptions middlewareInjectorOptions)
        {
            _sharedData = sharedData;
            _sharedService = sharedService;
            _middlewareInjectorOptions = middlewareInjectorOptions;
        }

        [HttpPost("StartApplication")]
        public ActionResult StartApplication()
        {
            return Ok();
        }

        [HttpDelete("DeleteHostData")]
        public ActionResult DeleteHostData()
        {
            _sharedService.UpdateAccountServer(ServerState.Stopped);
            _sharedData.hostData = null;
            return Ok();
        }

        [HttpPost("SaveHosData")]
        public ActionResult SaveHosData([FromBody] HostData hostData)
        {
            _sharedData.hostData = hostData;
            _sharedService.CheckAndRegisterGenres();
            _sharedService.InjectTranscodeMiddleware();
            return Ok();
        }
    }
}
