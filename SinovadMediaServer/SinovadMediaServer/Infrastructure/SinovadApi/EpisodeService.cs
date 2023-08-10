using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;

namespace SinovadMediaServer.Infrastructure.SinovadApi
{
    public class EpisodeService
    {
        private readonly SinovadApiService _sinovadApiService;

        public EpisodeService(SinovadApiService sinovadApiService)
        {
            _sinovadApiService= sinovadApiService;
        }

        public async Task<Response<List<EpisodeDto>>> GetAllAsync()
        {
            var response = new Response<List<EpisodeDto>>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<List<EpisodeDto>>(HttpMethodType.GET, "/episodes/GetAllAsync");
                response.Data = result.MapTo<List<EpisodeDto>>();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<Response<EpisodeDto>> GetTvEpisodeAsync(int tvSerieId,int seasonNumber,int episodeNumber)
        {
            var response = new Response<EpisodeDto>();
            try
            {
                var path= "/episodes/GetTvEpisodeAsync?tvSerieId="+tvSerieId+"&seasonNumber="+seasonNumber+"&episodeNumber="+episodeNumber;
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<EpisodeDto>(HttpMethodType.GET, path);
                response.Data = result.Data;
                response.IsSuccess = true;
                response.Message = "Successful";
            } catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

    }
}
