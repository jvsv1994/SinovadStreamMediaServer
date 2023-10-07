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

    }

}
