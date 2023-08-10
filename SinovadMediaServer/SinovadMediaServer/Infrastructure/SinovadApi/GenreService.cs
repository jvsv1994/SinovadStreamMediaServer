using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Domain.Enums;
using SinovadMediaServer.Transversal.Common;
using SinovadMediaServer.Transversal.Mapping;

namespace SinovadMediaServer.Infrastructure.SinovadApi
{
    public class GenreService
    {
        private readonly SinovadApiService _sinovadApiService;

        public GenreService(SinovadApiService sinovadApiService)
        {
            _sinovadApiService= sinovadApiService;
        }

        public async Task<Response<List<GenreDto>>> GetAllAsync()
        {
            var response = new Response<List<GenreDto>>();
            try
            {
                var result = await _sinovadApiService.ExecuteHttpMethodAsync<List<GenreDto>>(HttpMethodType.GET, "/genres/GetAllAsync");
                response.Data = result.MapTo<List<GenreDto>>();
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
