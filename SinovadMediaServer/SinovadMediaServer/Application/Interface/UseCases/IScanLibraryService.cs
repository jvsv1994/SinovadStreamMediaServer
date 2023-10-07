using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IScanLibraryService
    {
        Task<Response<object>> SearchFilesAsync(SearchFilesDto searchFilesDto);
    }
}
