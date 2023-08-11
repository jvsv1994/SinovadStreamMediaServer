
using SinovadMediaServer.Application.DTOs;

namespace SinovadMediaServer.Application.Interface.Infrastructure
{
    public interface IImdbService
    {
        MediaItemDto SearchMovie(string movieName, string year);
    }
}
