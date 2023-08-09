using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Infrastructure;
using SinovadMediaServer.Shared;
using SinovadMediaServer.Strategies;
using System.Text.Json;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/transcodeVideos")]
    public class TranscodeVideoController : Controller
    {

        private readonly SinovadApiService _sinovadApiService;

        private readonly SharedData _sharedData;

        public TranscodeVideoController(SinovadApiService restService, SharedData sharedData)
        {
            _sinovadApiService = restService;
            _sharedData = sharedData;
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(string guids)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                var transcodeProcessStrategy = new TranscodeProcessStrategy(_sharedData, _sinovadApiService);
                var list = await transcodeProcessStrategy.DeleteListByGuids(guids);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(list));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("UpdateVideoData")]
        public async Task<ActionResult> UpdateVideoData([FromBody] TranscodePrepareVideoDto transcodePrepareVideo)
        {
            try
            {
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_sinovadApiService, _sharedData);
                TranscodeRunVideoDto transcodeRunVideoDto = transcodeVideoStrategy.Run(transcodePrepareVideo);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transcodeRunVideoDto));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("GetVideoData")]
        public async Task<ActionResult> GetVideoData([FromBody] TranscodePrepareVideoDto transcodeVideoDto)
        {
            try
            {
                var transcodeVideoStrategy = new TranscodeVideoStrategy(_sinovadApiService, _sharedData);
                TranscodePrepareVideoDto transcodePrepareVideo = await transcodeVideoStrategy.Prepare(transcodeVideoDto);
                TranscodeRunVideoDto transcodeRunVideoDto = transcodeVideoStrategy.Run(transcodePrepareVideo);
                var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(transcodeRunVideoDto));
                return Ok(Convert.ToBase64String(plainTextBytes));
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
