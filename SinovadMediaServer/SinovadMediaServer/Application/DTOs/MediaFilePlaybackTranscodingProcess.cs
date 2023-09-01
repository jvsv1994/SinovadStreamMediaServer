#nullable disable
namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFilePlaybackTranscodingProcess
    {
        public int? TranscodeAudioVideoProcessId { get; set; }
        public int? TranscodeSubtitlesProcessId { get; set; }
        public string? TranscodeFolderName { get; set; }
        public string? TranscodeFolderPath { get; set; }
        public DateTime Created { get; set; }
    }
}
