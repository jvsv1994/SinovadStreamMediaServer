using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IMediaFilePlaybackService
    {
        Response<List<MediaFilePlaybackRealTimeDto>> GetListMediaFilePlaybackRealTime();
        Response<TranscodedMediaFileResponseDto> CreateTranscodedMediaFile(MediaFilePlaybackRealTimeDto mediaFilePlaybackRealTime,string clientIpAddress);
        Response<string> RetranscodeMediaFile(RetranscodeMediaFileRequestDto retranscodeVideoRequest);
        Response<bool> DeleteLastTranscodedMediaFileProcessByGuid(string guid);
        Response<bool> DeleteTranscodedMediaFileByGuid(string guid);
        Response<bool> DeleteAllTranscodedMediaFiles();
        Response<bool> DeleteOldTranscodedMediaFiles();
        Response<bool> UpdateAllMediaFileProfile();

    }
}
