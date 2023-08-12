using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Infrastructure.Imdb
{
    public class ImdbService : IImdbService
    {

        private readonly IMDbApiLib.ApiLib _imdbApiLib;

        private readonly string _apiKey = "k_aovgfcx9";

        public ImdbService()
        {
            _imdbApiLib = new IMDbApiLib.ApiLib(_apiKey);
        }

        public MediaItemDto SearchMovie(string movieName, string year)
        {
            IMDbApiLib.Models.TitleData titleData = null;
            IMDbApiLib.Models.SearchData result = _imdbApiLib.SearchMovieAsync(movieName + " " + year).Result;
            if (result != null && result.Results != null && result.Results.Count > 0)
            {
                IMDbApiLib.Models.SearchResult search = result.Results[0];
                string imdbID = search.Id;
                titleData = _imdbApiLib.TitleAsync(imdbID, IMDbApiLib.Models.Language.es).Result;
            }

            MediaItemDto movieDto = null;
            if (titleData != null)
            {
                var releaseDate = DateTime.Parse(titleData.ReleaseDate);
                movieDto = new MediaItemDto();
                movieDto.Title = titleData.Title;
                movieDto.ExtendedTitle = titleData.Title + " (" + releaseDate.Year + ")";
                movieDto.Overview = titleData.PlotLocal;
                movieDto.PosterPath = titleData.Image;
                movieDto.SourceId = titleData.Id;
                movieDto.ReleaseDate = releaseDate;
                movieDto.Title = titleData.Title;
                movieDto.MediaTypeId = MediaType.Movie;
                movieDto.MetadataAgentsId = MetadataAgents.IMDb;
                movieDto.SearchQuery = movieName;
                if (titleData.ActorList != null)
                {
                    movieDto.Actors = string.Join(", ", titleData.ActorList.Select(x => x.Name));
                }
                if (titleData.DirectorList!=null)
                {
                    movieDto.Directors = string.Join(", ", titleData.DirectorList.Select(x => x.Name));
                }
                if (titleData.GenreList != null && titleData.GenreList.Count > 0)
                {
                    movieDto.Genres = string.Join(", ", titleData.GenreList.Select(x => x.Key));
                    movieDto.ListGenreNames = titleData.GenreList.Select(it => it.Key).ToList();
                }
            }
            return movieDto;
        }

    }
}
