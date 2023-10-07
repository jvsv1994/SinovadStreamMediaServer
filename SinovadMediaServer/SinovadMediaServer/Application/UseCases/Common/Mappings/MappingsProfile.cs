using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.DTOs.Library;
using SinovadMediaServer.Domain.Entities;

namespace SinovadMediaServer.Application.UseCases.Common.Mappings
{
    public class MappingsProfile : AutoMapper.Profile
    {
        public MappingsProfile()
        {
            CreateMap<Library, LibraryDto>().ReverseMap();
            CreateMap<LibraryCreationDto, Library>();

            CreateMap<TranscoderSettings, TranscoderSettingsDto>().ReverseMap();
            CreateMap<MediaItem, MediaItemDto>().ReverseMap();
            CreateMap<MediaSeason, MediaSeasonDto>().ReverseMap();
            CreateMap<MediaEpisode, MediaEpisodeDto>().ReverseMap();
            CreateMap<MediaGenre, MediaGenreDto>().ReverseMap();
            CreateMap<MediaItemGenre, MediaItemGenreDto>().ReverseMap();
            CreateMap<MediaFile, MediaFileDto>().ReverseMap();
            CreateMap<Alert, AlertDto>().ReverseMap();
            CreateMap<MediaFileProfile, MediaFileProfileDto>().ReverseMap();

        }
    }
}
