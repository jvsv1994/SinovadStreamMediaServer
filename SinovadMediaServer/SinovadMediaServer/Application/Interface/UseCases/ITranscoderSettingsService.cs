using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface ITranscoderSettingsService
    {
        Task<Response<TranscoderSettingsDto>> GetAsync();
        Task<Response<TranscoderSettingsDto>> Save(TranscoderSettingsDto transcoderSettingsDto);
    }

}
