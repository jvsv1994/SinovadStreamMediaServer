using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.UseCases.Common.Mappings
{
    public class MappingsProfile : AutoMapper.Profile
    {
        public MappingsProfile()
        {
            CreateMap<Library, LibraryDto>().ReverseMap();
            CreateMap<TranscoderSettings, TranscoderSettingsDto>().ReverseMap();
            CreateMap<TranscodingProcess, TranscodingProcessDto>().ReverseMap();
            CreateMap<MediaItem, MediaItemDto>().ReverseMap();
            CreateMap<MediaSeason, MediaSeasonDto>().ReverseMap();
            CreateMap<MediaEpisode, MediaEpisodeDto>().ReverseMap();
            CreateMap<MediaGenre, MediaGenreDto>().ReverseMap();
            CreateMap<MediaItemGenre, MediaItemGenreDto>().ReverseMap();
            CreateMap<MediaFile, MediaFileDto>().ReverseMap();
            CreateMap<Alert, AlertDto>().ReverseMap();
            CreateMap<MediaFilePlayback, MediaFilePlaybackDto>().ReverseMap();

        }
    }
}
