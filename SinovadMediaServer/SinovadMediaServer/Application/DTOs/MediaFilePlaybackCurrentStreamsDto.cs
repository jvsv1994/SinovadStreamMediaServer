#nullable disable
namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackCurrentStreamsDto
    {
        public int? TranscodeAudioVideoProcessId { get; set; }
        public int? TranscodeSubtitlesProcessId { get; set; }
        public string? TranscodeFolderName { get; set; }
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

    }
}
