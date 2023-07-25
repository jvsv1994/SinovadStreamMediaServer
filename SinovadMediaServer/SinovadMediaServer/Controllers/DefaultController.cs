using Microsoft.AspNetCore.Mvc;

namespace SinovadMediaServer.Controllers
{
    [Route("/")]
    [ApiController]
    public class DefaultController : ControllerBase
    {

        [HttpGet]
        public object Get()
        {
            return Redirect("/home");
        }

    }
}
