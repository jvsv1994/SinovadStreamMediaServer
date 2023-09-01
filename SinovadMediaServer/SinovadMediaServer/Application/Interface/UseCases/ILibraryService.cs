using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface ILibraryService
    {
        Task<Response<LibraryDto>> GetAsync(int id);
        Task<Response<List<LibraryDto>>> GetAllLibraries();
        Response<object> Create(LibraryDto libraryDto);
        Response<object> CreateList(List<LibraryDto> listLibraryDto);
        Response<object> Update(LibraryDto libraryDto);
        Response<object> Delete(int id);
        Response<object> DeleteList(string ids);
        Response<object> SearchFiles(SearchFilesDto searchFilesDto);
        Response<List<ItemsGroupDto>> GetMediaItemsByLibrary(int libraryId, int profileId);
        Response<List<ItemsGroupDto>> GetMediaItemsByMediaType(MediaType mediaTypeId, int profileId);
        Response<List<ItemsGroupDto>> GetAllMediaItems(int profileId);
        Response<List<ItemDto>> GetAllMediaItemsBySearchQuery(string searchQuery);
        Response<ItemDetailDto> GetMediaItemDetail(int mediaItemId);
        Response<ItemDetailDto> GetMediaItemDetailByMediaFileAndProfile(int mediaFileId, int profileId);

    }

}
