namespace SinovadMediaServer.Application.DTOs
{
    public class TranscodedMediaFileResponseDto
    {
        public string Guid { get; set; }
        public string Url { get; set; }
        public double InitialTime { get; set; }
        public double Duration { get; set; }
        public int VideoTransmissionTypeId { get; set; }
        public List<StreamDto> ListAudioStreams { get; set; }
        public List<StreamDto> ListSubtitleStreams { get; set; }

    }
}
