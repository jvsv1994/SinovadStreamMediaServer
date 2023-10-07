using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Application.Interface.UseCases;

namespace SinovadMediaServer.Controllers
{
    [ApiController]
    [Route("api/scanLibrary")]
    public class ScanLibraryController : Controller
    {
        private readonly IScanLibraryService _scanLibraryService;

        public ScanLibraryController(IScanLibraryService scanLibraryService)
        {
            _scanLibraryService = scanLibraryService;
        }

        [HttpPost]
        public async Task<ActionResult> Start([FromBody] SearchFilesDto searchFilesDto)
        {
            var response = await _scanLibraryService.SearchFilesAsync(searchFilesDto);
            if(!response.IsSuccess)
            {
                return BadRequest(response.Message);
            }
            return NoContent();
        }
    }
}
