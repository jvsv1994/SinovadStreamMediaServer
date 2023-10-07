using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Application.DTOs.Library;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface ILibraryService
    {
        Task<Response<LibraryDto>> GetAsync(int id);
        Task<Response<List<LibraryDto>>> GetAllAsync();
        Task<Response<LibraryDto>> CreateAsync(LibraryCreationDto libraryDto);
        Task<Response<object>> UpdateAsync(int id,LibraryCreationDto libraryDto);
        Task<Response<object>> DeleteAsync(int id);


        Response<object> SearchFiles(SearchFilesDto searchFilesDto);
        Response<List<ItemsGroupDto>> GetMediaItemsByLibrary(int libraryId, int profileId);
        Response<List<ItemsGroupDto>> GetMediaItemsByMediaType(MediaType mediaTypeId, int profileId);
        Response<List<ItemsGroupDto>> GetAllMediaItems(int profileId);
        Response<List<ItemDto>> GetAllMediaItemsBySearchQuery(string searchQuery);
        Response<ItemDetailDto> GetMediaItemDetail(int mediaItemId);
        Response<ItemDetailDto> GetMediaItemDetailByMediaFileAndProfile(int mediaFileId, int profileId);

    }

}
