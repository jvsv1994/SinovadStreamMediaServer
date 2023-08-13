#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class ItemDetailDto
    {
    
        public List<MediaSeasonDto> ListSeasons { get; set; }
        public List<MediaFileDto> ListMediaFiles { get; set; }
        public MediaItemDto MediaItem { get; set; }
        public MediaSeasonDto CurrentSeason { get; set; }
        public MediaEpisodeDto CurrentEpisode { get; set; }
        public MediaFilePlaybackDto LastMediaFilePlayback { get; set; }

    }
}
