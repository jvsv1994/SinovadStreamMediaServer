using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface ITranscodingProcessService
    {
        Task<Response<TranscodingProcessDto>> GetAsync(int id);
        Task<Response<List<TranscodingProcessDto>>> GetAllAsync();
        Task<Response<List<TranscodingProcessDto>>> GetAllByListGuidsAsync(string guids);
        Response<object> Create(TranscodingProcessDto transcodingProcessDto);
        Response<object> CreateList(List<TranscodingProcessDto> listTranscodingProcessDto);
        Response<object> Update(TranscodingProcessDto transcodingProcessDto);
        Response<object> Delete(int id);
        Response<object> DeleteList(string ids);
        Response<List<Guid>> DeleteByListGuids(string guids);
        Task<bool> DeleteOldTranscodeVideoProcess();
        Task<TranscodePrepareVideoDto> PrepareProcess(TranscodePrepareVideoDto transcodePrepareVideo);
        TranscodeRunVideoDto RunProcess(TranscodePrepareVideoDto transcodePrepareVideo);
    }

}
