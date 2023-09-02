using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.Controllers
{
    [Route("api/mediaFilePlaybacks")]
    [ApiController]
    public class MediaFilePlaybackController : ControllerBase
    {
        private readonly IMediaFilePlaybackService _mediaFilePlaybackService;

        public MediaFilePlaybackController(IMediaFilePlaybackService mediaFilePlaybackService)
        {
            _mediaFilePlaybackService = mediaFilePlaybackService;
        }

        [HttpGet("GetListMediaFilePlaybackRealTime")]
        public async Task<ActionResult> GetListMediaFilePlaybackRealTime()
        {
            try
            {
                var response = _mediaFilePlaybackService.GetListMediaFilePlaybackRealTime();
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPost("CreateTranscodedMediaFile")]
        public async Task<ActionResult> CreateTranscodedMediaFile([FromBody] MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime)
        {
            try
            {
                var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var response = _mediaFilePlaybackService.CreateTranscodedMediaFile(mediaFilePlaybackRealTime, clientIpAddress);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response.Message);
            }catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpPut("RetranscodeMediaFile")]
        public async Task<ActionResult> RetranscodeMediaFile([FromBody] RetranscodeMediaFileRequestDto retranscodeMediaFileRequest)
        {
            try
            {
                var response = _mediaFilePlaybackService.RetranscodeMediaFile(retranscodeMediaFileRequest);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("DeleteTranscodedMediaFileByGuid/{guid}")]
        public async Task<ActionResult> DeleteTranscodedMediaFileByGuid([FromRoute] string guid)
        {
            try
            {
                var response = _mediaFilePlaybackService.DeleteTranscodedMediaFileByGuid(guid);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpDelete("DeleteLastTranscodedMediaFileProcessByGuid/{guid}")]
        public async Task<ActionResult> DeleteLastTranscodedMediaFileProcessByGuid([FromRoute] string guid)
        {
            try
            {
                var response = _mediaFilePlaybackService.DeleteLastTranscodedMediaFileProcessByGuid(guid);
                if (response.IsSuccess)
                {
                    return Ok(response);
                }
                return BadRequest(response.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


    }
}
