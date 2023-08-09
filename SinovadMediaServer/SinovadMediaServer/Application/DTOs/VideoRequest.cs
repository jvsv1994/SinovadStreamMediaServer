using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Application.DTOs
{
    public class VideoRequest
    {
        public int LibraryId { get; set; }
        public MediaType MediaType { get; set; }
        public string Paths { get; set; }
        public string LogIdentifier { get; set; }

    }
}
