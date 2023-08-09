using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Application.DTOs;

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
    }

}
