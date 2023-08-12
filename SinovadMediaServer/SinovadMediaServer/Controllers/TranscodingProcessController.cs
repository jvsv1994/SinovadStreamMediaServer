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

        [HttpGet("GetAsync/{id}")]
        public async Task<ActionResult> GetAsync(int id)
        {
            var response = await _transcodingProcessService.GetAsync(id);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetAllAsync")]
        public async Task<ActionResult> GetAllAsync()
        {
            var response = await _transcodingProcessService.GetAllAsync();
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetAllByListGuidsAsync/{guids}")]
        public async Task<ActionResult> GetAllByListGuidsAsync(string guids)
        {
            var response = await _transcodingProcessService.GetAllByListGuidsAsync(guids);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("Create")]
        public ActionResult Create([FromBody] TranscodingProcessDto transcodingProcessDto)
        {
            var response = _transcodingProcessService.Create(transcodingProcessDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("CreateList")]
        public ActionResult CreateList([FromBody] List<TranscodingProcessDto> list)
        {
            var response = _transcodingProcessService.CreateList(list);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPut("Update")]
        public ActionResult Update([FromBody] TranscodingProcessDto transcodingProcessDto)
        {
            var response = _transcodingProcessService.Update(transcodingProcessDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("Delete/{transcodingProcessId}")]
        public ActionResult Delete(int transcodingProcessId)
        {
            var response = _transcodingProcessService.Delete(transcodingProcessId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("DeleteList/{listIds}")]
        public ActionResult DeleteList(string listIds)
        {
            var response = _transcodingProcessService.DeleteList(listIds);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
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
