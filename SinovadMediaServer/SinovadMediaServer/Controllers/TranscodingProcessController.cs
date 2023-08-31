using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using System.Text.Json;

namespace SinovadMediaServer.Controllers
{
    [Route("api/transcodingProcesses")]
    [ApiController]
    public class TranscodingProcessController : ControllerBase
    {
        private readonly ITranscodingProcessService _transcodingProcessService;

        public TranscodingProcessController(ITranscodingProcessService transcodingProcessService)
        {
            _transcodingProcessService = transcodingProcessService;
        }

        [HttpDelete("DeleteByListGuids")]
        public async Task<ActionResult> DeleteByListGuids([FromQuery] string guids)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();
                var list = _transcodingProcessService.DeleteByListGuids(guids);
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
                TranscodeRunVideoDto transcodeRunVideoDto = _transcodingProcessService.RunProcess(transcodePrepareVideo);
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
                TranscodePrepareVideoDto transcodePrepareVideo = await _transcodingProcessService.PrepareProcess(transcodeVideoDto);
                TranscodeRunVideoDto transcodeRunVideoDto = _transcodingProcessService.RunProcess(transcodePrepareVideo);
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
