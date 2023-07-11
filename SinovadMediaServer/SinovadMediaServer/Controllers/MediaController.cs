using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Strategies;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Shared;
using SinovadMediaServer.DTOs;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("medias")]
    public class MediaController : Controller
    {

        public static IOptions<MyConfig> _config { get; set; }

        private SharedData _sharedData;

        public MediaController(IOptions<MyConfig> config,SharedData sharedData)
        {
            _config= config;
            _sharedData= sharedData;
        }

        [HttpPost("UpdateVideosInListStorages")]
        public ActionResult UpdateVideosInAllStorages([FromBody] UpdateStorageVideosDto request)
        {
            try
            {
                var searhMovieVideoFileStrategy = new SearchVideoFilesStrategy(_config, _sharedData);
                searhMovieVideoFileStrategy.UpdateVideosInListStorages(request);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
