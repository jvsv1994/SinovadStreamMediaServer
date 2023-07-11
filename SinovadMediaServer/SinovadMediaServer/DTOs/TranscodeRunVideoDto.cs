using SinovadMediaServer.CustomModels;

#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class TranscodeRunVideoDto
    {

        public int TranscodeProcessStreamId { get; set; }
        public int TranscodeProcessSubtitlesId { get; set; }
        public string VideoPath { get; set; }
        public string VideoOutputDirectoryPhysicalPath { get; set; }
        public string VideoOutputDirectoryRoutePath { get; set; }
        public TranscodePrepareVideoDto TranscodePrepareVideo { get; set;}

    }
}
