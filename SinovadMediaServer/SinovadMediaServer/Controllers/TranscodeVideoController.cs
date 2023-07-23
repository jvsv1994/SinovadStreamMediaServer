using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Proxy;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using System.Text.Json;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("transcodeVideos")]
    public class TranscodeVideoController : Controller
    {
        public readonly IOptions<MyConfig> _config;

        private readonly RestService _restService;

        public TranscodeVideoController(IOptions<MyConfig> config, SharedData sharedData, RestService restService)
        {
            _config = config;
            _restService = restService;
            string authorization = Request.Headers["Authorization"];
            if(authorization != null)
            {
                var authValues = authorization.Split(" ");
                if (authValues.Length > 1) {
                   var apiKey = authValues[1];
                   sharedData.ApiToken = apiKey;
                }
            }
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string guids)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, Object>();
                var transcodeProcessStrategy = new TranscodeProcessStrategy(_restService);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transcodeProcessStrategy.deleteList(guids)));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("UpdateVideoData")]
        public async Task<ActionResult> UpdateVideoData([FromBody] TranscodePrepareVideoDto transcodePrepareVideo)
        {
            try
            {
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_config,_restService);
                TranscodeRunVideoDto transcodeRunVideoDto = transcodeVideoStrategy.Run(transcodePrepareVideo);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transcodeRunVideoDto));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("GetVideoData")]
        public async Task<ActionResult> GetVideoData([FromBody] TranscodePrepareVideoDto transcodeVideoDto)
        {
            try
            {
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_config, _restService);
                TranscodePrepareVideoDto transcodePrepareVideo = await transcodeVideoStrategy.Prepare(transcodeVideoDto);
                TranscodeRunVideoDto transcodeRunVideoDto= transcodeVideoStrategy.Run(transcodePrepareVideo);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transcodeRunVideoDto));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
