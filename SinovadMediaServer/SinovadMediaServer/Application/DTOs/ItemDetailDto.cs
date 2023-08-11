using SinovadMediaServer.Domain.Enums;


#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class ItemDetailDto
    {
    
        public List<MediaSeasonDto> ListSeasons { get; set; }
        public List<MediaFileDto> ListMediaFiles { get; set; }
        public MediaItemDto MediaItem { get; set; }

    }
}
