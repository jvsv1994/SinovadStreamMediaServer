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

        [HttpGet("GetListMediaFilePlayback")]
        public async Task<ActionResult> GetListMediaFilePlayback()
        {
            try
            {
                var response = _mediaFilePlaybackService.GetListMediaFilePlayback();
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
        public async Task<ActionResult> CreateTranscodedMediaFile([FromBody] MediaFilePlaybackDto MediaFilePlayback)
        {
            try
            {
                var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                var response = _mediaFilePlaybackService.CreateTranscodedMediaFile(MediaFilePlayback, clientIpAddress);
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

    }
}
