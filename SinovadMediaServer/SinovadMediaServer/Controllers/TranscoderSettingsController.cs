using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/transcoderSettings")]
    public class TranscoderSettingsController : Controller
    {
        private readonly ITranscoderSettingsService _transcoderSettingsService;

        public TranscoderSettingsController(ITranscoderSettingsService transcoderSettingsService)
        {
            _transcoderSettingsService = transcoderSettingsService;
        }

        [HttpGet("GetAsync")]
        public async Task<ActionResult> GetAsync()
        {
            var response = await _transcoderSettingsService.GetAsync();
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPut("Save")]
        public async Task<ActionResult> Save([FromBody] TranscoderSettingsDto transcoderSettingsDto)
        {
            if (transcoderSettingsDto == null)
            {
                return BadRequest();
            }
            var response = await _transcoderSettingsService.Save(transcoderSettingsDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

    }
}
