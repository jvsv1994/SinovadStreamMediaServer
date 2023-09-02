using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Application.Interface.UseCases
{
    public interface IMediaFilePlaybackService
    {
        Response<List<MediaFilePlaybackDto>> GetListMediaFilePlayback();
        Response<TranscodedMediaFileResponseDto> CreateTranscodedMediaFile(MediaFilePlaybackDto MediaFilePlayback,string clientIpAddress);
        Response<string> RetranscodeMediaFile(RetranscodeMediaFileRequestDto retranscodeVideoRequest);
        void UpdateAllMediaFileProfile();

    }
}
