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

        [HttpGet("GetAsync/{id}")]
        public async Task<ActionResult> GetByMediaServerAsync(int id)
        {
            var response = await _transcoderSettingsService.GetAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("Create")]
        public ActionResult Create([FromBody] TranscoderSettingsDto transcoderSettingsDto)
        {
            var response = _transcoderSettingsService.Create(transcoderSettingsDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("CreateList")]
        public ActionResult CreateList([FromBody] List<TranscoderSettingsDto> list)
        {
            var response = _transcoderSettingsService.CreateList(list);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPut("Update")]
        public ActionResult Update([FromBody] TranscoderSettingsDto transcoderSettingsDto)
        {
            var response = _transcoderSettingsService.Update(transcoderSettingsDto);
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

        [HttpDelete("Delete/{transcoderSettingsId}")]
        public ActionResult Delete(int transcoderSettingsId)
        {
            var response = _transcoderSettingsService.Delete(transcoderSettingsId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("DeleteList/{listIds}")]
        public ActionResult DeleteList(string listIds)
        {
            var response = _transcoderSettingsService.DeleteList(listIds);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

    }
}
