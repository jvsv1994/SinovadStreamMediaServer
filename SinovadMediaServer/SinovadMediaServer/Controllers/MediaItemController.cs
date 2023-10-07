using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/mediaItems")]
    public class MediaItemController : Controller
    {
        private readonly ILibraryService _libraryService;

        public MediaItemController(ILibraryService libraryService)
        {
            _libraryService = libraryService;
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
            var response = _libraryService.GetAllMediaItems(profileId);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

        [HttpGet("GetMediaItemsByMediaType")]
        public ActionResult GetMediaItemsByMediaType([FromQuery] int mediaTypeId, [FromQuery] int profileId)
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

        [HttpGet("GetMediaItemDetailByMediaFileAndProfile")]
        public ActionResult GetMediaItemDetailByMediaFileAndProfile([FromQuery] int mediaFileId, [FromQuery] int profileId)
        {
            var response = _libraryService.GetMediaItemDetailByMediaFileAndProfile(mediaFileId, profileId);
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

    }
}
