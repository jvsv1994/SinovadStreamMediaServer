using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;

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

        [HttpGet("GetAllByMediaServerAsync/{mediaServerId}")]
        public async Task<ActionResult> GetAllByMediaServerAsync(int mediaServerId)
        {
            var response = await _transcodingProcessService.GetAllByMediaServerAsync(mediaServerId);
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

        [HttpDelete("DeleteByListGuids/{guids}")]
        public ActionResult DeleteByListGuids(string guids)
        {
            var response = _transcodingProcessService.DeleteByListGuids(guids);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

    }
}
