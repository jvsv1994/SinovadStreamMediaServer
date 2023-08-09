using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface ITranscoderSettingsService
    {
        Task<Response<TranscoderSettingsDto>> GetAsync(int id);
        Response<object> Create(TranscoderSettingsDto transcoderSettingsDto);
        Response<object> CreateList(List<TranscoderSettingsDto> listTranscoderSettingsDto);
        Response<object> Update(TranscoderSettingsDto transcoderSettingsDto);
        Task<Response<TranscoderSettingsDto>> Save(TranscoderSettingsDto transcoderSettingsDto);
        Response<object> Delete(int id);
        Response<object> DeleteList(string ids);
    }

}
