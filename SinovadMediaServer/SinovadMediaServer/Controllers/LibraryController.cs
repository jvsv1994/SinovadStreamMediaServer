using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Enums;

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

        [HttpGet("GetAllLibraries")]
        public async Task<ActionResult> GetAllLibraries()
        {
            var response = await _libraryService.GetAllLibraries();
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("Create")]
        public ActionResult Create([FromBody] LibraryDto libraryDto)
        {
            var response = _libraryService.Create(libraryDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("CreateList")]
        public ActionResult CreateList([FromBody] List<LibraryDto> list)
        {
            var response = _libraryService.CreateList(list);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPut("Update")]
        public ActionResult Update([FromBody] LibraryDto seasonDto)
        {
            var response = _libraryService.Update(seasonDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("Delete/{libraryId}")]
        public ActionResult Delete(int libraryId)
        {
            var response = _libraryService.Delete(libraryId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpDelete("DeleteList/{listIds}")]
        public ActionResult DeleteList(string listIds)
        {
            var response = _libraryService.DeleteList(listIds);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPost("SearchFiles")]
        public ActionResult SearchFiles([FromBody] SearchFilesDto searchFilesDto)
        {
            var response = _libraryService.SearchFiles(searchFilesDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetMediaItemsByLibrary")]
        public ActionResult GetMediaItemsByLibrary([FromQuery] int libraryId, [FromQuery] int profileId)
        {
            var response = _libraryService.GetMediaItemsByLibrary(libraryId, profileId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetAllMediaItems")]
        public ActionResult GetAllMediaItems([FromQuery] int profileId)
        {
            var response = _libraryService.GetAllMediaItems( profileId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetMediaItemsByMediaType")]
        public ActionResult GetMediaItemsByMediaType([FromQuery] int mediaTypeId, [FromQuery]int profileId)
        {
            var response = _libraryService.GetMediaItemsByMediaType((MediaType)mediaTypeId, profileId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetMediaItemDetail/{mediaItemId}")]
        public ActionResult GetMediaItemDetail([FromRoute] int mediaItemId)
        {
            var response = _libraryService.GetMediaItemDetail(mediaItemId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }


        [HttpGet("GetAllMediaItemsBySearchQuery")]
        public ActionResult GetAllMediaItemsBySearchQuery([FromQuery] string searchQuery)
        {
            var response = _libraryService.GetAllMediaItemsBySearchQuery(searchQuery);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpPut("UpdateMediaFilePlayback")]
        public ActionResult UpdateMediaFilePlayback([FromBody] MediaFilePlaybackDto mediaFilePlayback)
        {
            var response = _libraryService.UpdateMediaFilePlayback(mediaFilePlayback);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

    }
}
