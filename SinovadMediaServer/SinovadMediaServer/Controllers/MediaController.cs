﻿using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Strategies;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.Shared;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Proxy;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("medias")]
    public class MediaController : Controller
    {

        private readonly RestService _restService;

        public MediaController(SharedData sharedData, RestService restService)
        {
            _restService = restService;
            string authorization = Request.Headers["Authorization"];
            if (authorization != null)
            {
                var authValues = authorization.Split(" ");
                if (authValues.Length > 1)
                {
                    var apiKey = authValues[1];
                    sharedData.ApiToken = apiKey;
                }
            }
        }

        [HttpPost("UpdateVideosInListStorages")]
        public ActionResult UpdateVideosInAllStorages([FromBody] UpdateStorageVideosDto request)
        {
            try
            {
                var searhMovieVideoFileStrategy = new SearchVideoFilesStrategy(_restService);
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
