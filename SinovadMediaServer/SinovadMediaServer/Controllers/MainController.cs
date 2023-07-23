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
            string authorization = Request.Headers["Authorization"];
            if (authorization != null)
            {
                var authValues = authorization.Split(" ");
                if (authValues.Length > 1)
                {
                    var apiKey = authValues[1];
                    _sharedData.ApiToken = apiKey;
                }
            }
        }

        [HttpPost("StartApplication")]
        public ActionResult StartApplication()
        {
            return Ok();
        }

        [HttpDelete("DeleteMediaServerData")]
        public ActionResult DeleteMediaServerData()
        {
            _sharedService.UpdateMediaServer(MediaServerState.Stopped);
            return Ok();
        }

        [HttpPost("SaveMediaServerData")]
        public ActionResult SaveMediaServerData([FromBody] MediaServerData MediaServerData)
        {
            _sharedService.CheckAndRegisterGenres();
            _sharedService.InjectTranscodeMiddleware();
            return Ok();
        }
    }
}
