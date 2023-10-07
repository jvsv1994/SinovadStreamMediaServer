using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs.Library;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/libraries")]
    public class LibraryController : Controller
    {
        private readonly ILibraryService _libraryService;

        public LibraryController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
        }

        [HttpGet("GetAsync/{id:int}",Name ="GetLibrary")]
        public async Task<ActionResult<Response<LibraryDto>>> GetAsync([FromRoute] int id)
        {
            var response = await _libraryService.GetAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetAllAsync")]
        public async Task<ActionResult<Response<List<LibraryDto>>>> GetAllAsync()
        {
            var response = await _libraryService.GetAllAsync();
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpPost("CreateAsync")]
        public async Task<ActionResult> CreateAsync([FromBody] LibraryCreationDto libraryCreationDto)
        {
            var response = await _libraryService.CreateAsync(libraryCreationDto);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return CreatedAtRoute("GetLibrary", new {id= response.Data.Id },response.Data);
        }

        [HttpPut("UpdateAsync/{id:int}")]
        public async Task<ActionResult> UpdateAsync([FromRoute] int id,[FromBody] LibraryCreationDto libraryCreationDto)
        {
            var response = await _libraryService.UpdateAsync(id, libraryCreationDto);
            if(!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return NoContent();
        }

        [HttpDelete("DeleteAsync/{id:int}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var response = await _libraryService.DeleteAsync(id);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return NoContent();
        }

    }
}
