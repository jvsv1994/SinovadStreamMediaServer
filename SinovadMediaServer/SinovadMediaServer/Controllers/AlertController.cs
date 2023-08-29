using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/alerts")]
    public class AlertController:Controller
    {

        private readonly IAlertService _alertService;

        public AlertController(IAlertService alertService)
        {
            _alertService = alertService;
        }

        [HttpGet("GetAllWithPagination")]
        public async Task<ActionResult> GetAllWithPagination(int page,int take)
        {
            var response = await _alertService.GetAllWithPagination(page, take);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response.Message);
        }

    }
}
