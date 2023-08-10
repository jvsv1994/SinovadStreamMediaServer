using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;

namespace SinovadMediaServer.Infrastructure.SinovadApi
{
    public class MovieService
    {
        private readonly SinovadApiService _sinovadApiService;

        public MovieService(SinovadApiService sinovadApiService)
        {
            _sinovadApiService= sinovadApiService;
        }

        public async Task<Response<List<MovieDto>>> GetAllAsync()
        {
            var response = new Response<List<MovieDto>>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<List<MovieDto>>(HttpMethodType.GET, "/movies/GetAllAsync");
                response.Data = result.MapTo<List<MovieDto>>();
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
