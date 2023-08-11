using Microsoft.Extensions.Options;
using SinovadMediaServer.Application.Configuration;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.Infrastructure;
using SinovadMediaServer.Domain.Enums;

namespace SinovadMediaServer.Infrastructure.Imdb
{
    public class ImdbService : IImdbService
    {

        private readonly IMDbApiLib.ApiLib _imdbApiLib;

        public ImdbService(IOptions<MyConfig> options)
        {
            _imdbApiLib = new IMDbApiLib.ApiLib(options.Value.IMDbApiKey);
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
                movieDto = new MediaItemDto();
                movieDto.Title = titleData.Title;
                movieDto.Overview = titleData.PlotLocal;
                movieDto.PosterPath = titleData.Image;
                movieDto.SourceId = titleData.Id;
                movieDto.ReleaseDate = DateTime.Parse(titleData.ReleaseDate);
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

        public ItemDetailDto GetMovieDetail(ItemDetailDto movieDetail)
        {
            IMDbApiLib.Models.TitleData titleData = _imdbApiLib.TitleAsync(movieDetail.Imdbid, IMDbApiLib.Models.Language.es).Result;
            movieDetail.Genres = titleData.Genres;
            movieDetail.Actors = string.Join(",", titleData.ActorList.Select(item => item.Name));
            movieDetail.Directors = titleData.Directors;
            return movieDetail;
        }
    }
}
