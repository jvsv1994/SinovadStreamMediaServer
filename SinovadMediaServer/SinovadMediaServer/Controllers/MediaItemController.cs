using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/mediaItems")]
    public class MediaItemController : Controller
    {
        private readonly IMediaItemService _mediaItemService;

        public MediaItemController(IMediaItemService mediaItemService)
        {
            _mediaItemService = mediaItemService;
        }

        [HttpGet("GetMediaItemsByLibrary")]
        public async Task<ActionResult<Response<List<ItemsGroupDto>>>> GetMediaItemsByLibrary([FromQuery] int libraryId, [FromQuery] int profileId)
        {
            var response = await _mediaItemService.GetMediaItemsByLibrary(libraryId, profileId);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetAllMediaItems")]
        public async Task<ActionResult<Response<List<ItemsGroupDto>>>> GetAllMediaItems([FromQuery] int profileId)
        {
            var response = await _mediaItemService.GetAllMediaItems(profileId);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetMediaItemsByMediaType")]
        public async Task<ActionResult<Response<List<ItemsGroupDto>>>> GetMediaItemsByMediaType([FromQuery] int mediaTypeId, [FromQuery] int profileId)
        {
            var response = await _mediaItemService.GetMediaItemsByMediaType((MediaType)mediaTypeId, profileId);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetMediaItemDetail/{mediaItemId}")]
        public async Task<ActionResult<Response<ItemDetailDto>>> GetMediaItemDetail([FromRoute] int mediaItemId)
        {
            var response = await _mediaItemService.GetMediaItemDetail(mediaItemId);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetMediaItemDetailByMediaFileAndProfile")]
        public async Task<ActionResult<Response<ItemDetailDto>>> GetMediaItemDetailByMediaFileAndProfile([FromQuery] int mediaFileId, [FromQuery] int profileId)
        {
            var response = await _mediaItemService.GetMediaItemDetailByMediaFileAndProfile(mediaFileId, profileId);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

        [HttpGet("GetAllMediaItemsBySearchQuery")]
        public async Task<ActionResult<Response<List<ItemDto>>>> GetAllMediaItemsBySearchQuery([FromQuery] string searchQuery)
        {
            var response = await _mediaItemService.GetAllMediaItemsBySearchQuery(searchQuery);
            if (!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return response;
        }

    }
}
