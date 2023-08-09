using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/medias")]
    public class MediaController : Controller
    {

        private readonly RestService _restService;

        public MediaController(SharedData sharedData, RestService restService)
        {
            _restService = restService;
            //string authorization = Request.Headers["Authorization"];
            //if (authorization != null)
            //{
            //    var authValues = authorization.Split(" ");
            //    if (authValues.Length > 1)
            //    {
            //        var apiKey = authValues[1];
            //        sharedData.ApiToken = apiKey;
            //    }
            //}
        }

        [HttpPost("UpdateVideosInListLibraries")]
        public ActionResult UpdateVideosInListLibraries([FromBody] UpdateLibraryVideosDto request)
        {
            try
            {
                var searhMovieVideoFileStrategy = new SearchVideoFilesStrategy(_restService);
                searhMovieVideoFileStrategy.UpdateVideosInListLibraries(request);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
