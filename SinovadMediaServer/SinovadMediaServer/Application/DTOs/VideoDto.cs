using SinovadMediaServer.Domain.Enums;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class VideoDto
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string PhysicalPath { get; set; }
        public int UserId { get; set; }
        public int MediaServerId { get; set; }
        public int LibraryId { get; set; }
        public MediaType MediaType { get; set; }
        public int VideoId { get; set; }
        public int MovieId { get; set; }
        public int TvSerieId { get; set; }
        public int EpisodeId { get; set; }
        public int? TmdbId { get; set; }
        public string Imdbid { get; set; }
        public int Year { get; set; }
        public int EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string EpisodeName { get; set; }
        public string MediaServerUrl { get; set; }

    }
}
