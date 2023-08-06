using Microsoft.AspNetCore.Mvc;

namespace SinovadMediaServer.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {

        [HttpGet]
        public string Get()
        {
            return "Running";
        }

    }
}
