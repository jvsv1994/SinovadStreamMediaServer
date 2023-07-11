using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace SinovadMediaServer.DTOs
{
    public class TranscodeVideoProcessDto
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public int TranscodeAudioVideoProcessId { get; set; }
        public int? TranscodeSubtitlesProcessId { get; set; }
        public string WorkingDirectoryPath { get; set; }
        public int AccountServerId { get; set; }
        public DateTime Created { get; set; }

    }
}
