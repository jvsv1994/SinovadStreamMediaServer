using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;

namespace SinovadMediaServer.Infrastructure.SinovadApi
{

    public class SinovadDbGenre
    {
        public int Id { get; set; }
        public string Name { get; set; }

    }

    public partial class SinovadDbTvSerie
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string? OriginalLanguage { get; set; }

        public string? OriginalName { get; set; }

        public string? Overview { get; set; }

        public double? Popularity { get; set; }

        public string? PosterPath { get; set; }

        public string? BackdropPath { get; set; }

        public DateTime? FirstAirDate { get; set; }

        public DateTime? LastAirDate { get; set; }

        public string? Directors { get; set; }

        public string? Actors { get; set; }
        public List<SinovadDbGenre> ListGenres { get; set; }

    }


    public class SinovadDbSeason
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; }
        public string? Name { get; set; }
        public string? Summary { get; set; }
        public int TvSerieId { get; set; }

    }


    public class SinovadDbEpisode
    {
        public int Id { get; set; }
        public int EpisodeNumber { get; set; }
        public string? Title { get; set; }

        public string? Summary { get; set; }

        public int SeasonId { get; set; }

        public string? ImageUrl { get; set; }

    }

    public class SinovadMediaDatabaseService {


        private readonly SinovadApiService _sinovadApiService;

        public SinovadMediaDatabaseService(SinovadApiService sinovadApiService)
        {
            _sinovadApiService = sinovadApiService;
        }

        public async Task<Response<SinovadDbTvSerie>> SearchTvSerieAsync(string query)
        {
            var response = new Response<SinovadDbTvSerie>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<SinovadDbTvSerie>(HttpMethodType.GET, "/tvseries/SearchAsync/" + query);
                response.Data = result.Data;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<SinovadDbSeason>> GetTvSeasonAsync(int tvSerieId, int seasonNumber)
        {
            var response = new Response<SinovadDbSeason>();
            try
            {
                var path = "/seasons/GetTvSeasonAsync?tvSerieId=" + tvSerieId + "&seasonNumber=" + seasonNumber;
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<SinovadDbSeason>(HttpMethodType.GET, path);
                response.Data = result.Data;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<SinovadDbEpisode>> GetTvEpisodeAsync(int tvSerieId, int seasonNumber, int episodeNumber)
        {
            var response = new Response<SinovadDbEpisode>();
            try
            {
                var path = "/episodes/GetTvEpisodeAsync?tvSerieId=" + tvSerieId + "&seasonNumber=" + seasonNumber + "&episodeNumber=" + episodeNumber;
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<SinovadDbEpisode>(HttpMethodType.GET, path);
                response.Data = result.Data;
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

    }
}
