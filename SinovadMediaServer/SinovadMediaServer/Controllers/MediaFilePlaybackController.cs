using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using System.Text.Json;

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

        [HttpPost("Create")]
        public async Task<ActionResult> Create([FromBody] MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime)
        {
            try
            {
         
                return Ok();
            }catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
