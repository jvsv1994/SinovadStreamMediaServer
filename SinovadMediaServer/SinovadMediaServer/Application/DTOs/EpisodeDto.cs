#nullable disable

using SinovadMediaServer;

namespace SinovadMediaServer.Application.DTOs
{
    public class EpisodeDto
    {
        public int Id { get; set; }
        public int MediaServerId { get; set; }
        public int? EpisodeNumber { get; set; }
        public int SeasonNumber { get; set; }
        public string Name { get; set; }
        public string Overview { get; set; }
        public string PhysicalPath { get; set; }
        public string StillPath { get; set; }
        public int SeasonId { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public int VideoId { get; set; }
        public int TmdbId { get; set; }
        public int TvSerieTmdbId { get; set; }
        public string ImageUrl { get; set; }
        public int TvSerieId { get; set; }
        public string TvSerieName { get; set; }
        public int EpisodeId { get; set; }
        public string MediaServerUrl { get; set; }

    }
}
