using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IAlertService
    {
        Task<Response<bool>> Create(string description, AlertType alertType);
        Task<ResponsePagination<List<AlertDto>>> GetAllWithPagination(int page, int take);

    }
}
