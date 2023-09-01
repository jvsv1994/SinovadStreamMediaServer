#nullable disable
using System.Security.Policy;

namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackCurrentStreamsDto
    {
        public string? OutputTranscodedFileName { get; set; }
        public bool? IsTranscodedAudio { get; set; }
        public string? CurrentAudioCodeFormatSource { get; set; }
        public string? CurrentAudioCodeFormatTarget { get; set; }
        public bool? IsTranscodedVideo { get; set; }
        public string? CurrentVideoCodeFormatSource { get; set; }
        public string? CurrentVideoCodeFormatTarget { get; set; }
        public string? CommandGenerateAudioVideoStreams { get; set; }
        public string? CommandGenerateSubtitleStreams { get; set; }
        public List<StreamDto>? ListAudioStreams { get; set; }
        public List<StreamDto>? ListSubtitleStreams { get; set; }
        public int VideoTransmissionTypeId { get; set; }
        public double Duration { get; set; }
        public List<MediaFilePlaybackTranscodingProcess> ListMediaFilePlaybackTranscodingProcess { get; set; }

    }
}
