#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class MovieGenreDto
    {
        public int MovieId { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
        public int TmdbId { get; set; }
        public string ImdbId { get; set; }

    }
}
