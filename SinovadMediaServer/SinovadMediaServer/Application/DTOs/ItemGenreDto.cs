using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace SinovadMediaServer.Application.DTOs
{
    public class ItemGenreDto
    {
        public int Id { get; set; }
        public int MovieId { get; set; }
        public int TvSerieId { get; set; }
        public int GenreId { get; set; }
        public string GenreName { get; set; }
    }
}
