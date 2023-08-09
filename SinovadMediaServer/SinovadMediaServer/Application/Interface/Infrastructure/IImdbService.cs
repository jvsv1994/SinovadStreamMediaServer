
using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Application.Interface.Infrastructure
{
    public interface IImdbService
    {
        MovieDto SearchMovie(string movieName, string year);
        ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail);
    }
}
