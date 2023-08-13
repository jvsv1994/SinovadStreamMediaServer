using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.DTOs
{
    public class MediaFileDto
    {
        public int Id { get; set; }
        public int? MediaItemId { get; set; }
        public int? MediaEpisodeId { get; set; }
        public int? LibraryId { get; set; }
        public string? PhysicalPath { get; set; }
        public MediaFilePlayback? MediaFilePlaytback { get; set; }

    }
}
