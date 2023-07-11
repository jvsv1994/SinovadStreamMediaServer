using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SinovadMediaServer.Configuration;
using SinovadMediaServer.DTOs;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using System.Text.Json;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("transcodeVideos")]
    public class TranscodeVideoController : Controller
    {
        public static IOptions<MyConfig> _config { get; set; }

        private SharedData _sharedData;

        public TranscodeVideoController(IOptions<MyConfig> config, SharedData sharedData)
        {
            _config = config;
            _sharedData= sharedData;
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string guids)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, Object>();
                var transcodeProcessStrategy = new TranscodeProcessStrategy(_config, _sharedData);
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
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_config, _sharedData);
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
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_config, _sharedData);
                TranscodePrepareVideoDto transcodePrepareVideo = transcodeVideoStrategy.Prepare(transcodeVideoDto);
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
