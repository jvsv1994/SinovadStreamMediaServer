using SinovadMediaServer.CustomModels;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class TranscodePrepareVideoDto
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public int VideoId { get; set; }
        public string MediaServerUrl { get; set; }
        public double CurrentTime { get; set; }
        public int MediaServerId { get; set; }
        public string PhysicalPath { get; set; }
        public Guid ProcessGUID { get; set; }
        public string TimeSpan { get; set; }
        public double TotalSeconds { get; set; }
        public string TranscodeDirectoryPhysicalPath { get; set; }
        public string TranscodeDirectoryRoutePath { get; set; }
        public string VideoOutputName { get; set; }
        public string FinalCommandGenerateStreams { get; set; }
        public string FinalCommandGenerateSubtitles { get; set; }
        public List<CustomStream> ListAudioStreams { get; set; }
        public List<CustomStream> ListSubtitlesStreams { get; set; }
        public int TransmissionMethodId { get; set; }
        public string Preset { get; set; }

    }
}
