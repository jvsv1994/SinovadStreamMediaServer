using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;

namespace SinovadMediaServer.Infrastructure.SinovadApi
{
    public class TvSerieService
    {
        private readonly SinovadApiService _sinovadApiService;

        public TvSerieService(SinovadApiService sinovadApiService)
        {
            _sinovadApiService= sinovadApiService;
        }

        public async Task<Response<List<TvSerieDto>>> GetAllAsync()
        {
            var response = new Response<List<TvSerieDto>>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<List<TvSerieDto>>(HttpMethodType.GET, "/tvseries/GetAllAsync");
                response.Data = result.MapTo<List<TvSerieDto>>();
                response.IsSuccess = true;
                response.Message = "Successful";
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<Response<TvSerieDto>> SearchAsync(string query)
        {
            var response = new Response<TvSerieDto>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<TvSerieDto>(HttpMethodType.GET, "/tvseries/SearchAsync/"+ query);
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
