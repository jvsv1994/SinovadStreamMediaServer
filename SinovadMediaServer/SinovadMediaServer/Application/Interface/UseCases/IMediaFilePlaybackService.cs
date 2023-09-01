using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IMediaFilePlaybackService
    {
        Response<TranscodedMediaFileResponseDto> CreateTranscodedMediaFile(MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime,string clientIpAddress);
        Response<string> RetranscodeMediaFile(RetranscodeMediaFileRequestDto retranscodeVideoRequest);
        Response<bool> UpdateMediaFilePlayback(UpdateMediaFilePlaybackRequestDto updateMediaFilePlaybackData);
        Response<bool> DeleteLastTranscodedMediaFileProcessByGuid(string guid);
        Response<bool> DeleteTranscodedMediaFileByGuid(string guid);
        Response<bool> DeleteAllTranscodedMediaFiles();
        Response<bool> DeleteOldTranscodedMediaFiles();

    }
}
