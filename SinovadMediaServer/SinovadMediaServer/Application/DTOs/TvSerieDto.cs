#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class TvSerieDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? TmdbId { get; set; }
        public string OriginalLanguage { get; set; }
        public string OriginalName { get; set; }
        public string Overview { get; set; }
        public double? Popularity { get; set; }
        public string PosterPath { get; set; }
        public string BackdropPath { get; set; }
        public DateTime? FirstAirDate { get; set; }
        public DateTime? LastAirDate { get; set; }
        public string Directors { get; set; }
        public string Genres { get; set; }
        public string Actors { get; set; }
        public List<SeasonDto> ListSeasons { get; set; }
        public List<TvSerieGenreDto> ListItemGenres { get; set; }
        public List<string> ListGenreNames { get; set; }
        public virtual List<TvSerieGenreDto> TvSerieGenres { get; set; }

    }
}
