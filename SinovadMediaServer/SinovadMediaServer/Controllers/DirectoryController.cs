using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.CustomModels;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.Controllers
{
    [Route("api/directories")]
    public class DirectoryController : Controller
    {

        [HttpGet]
        public async Task<ActionResult<List<CustomDirectory>>> GetMainDirectories()
        {
            try
            {
                var manageDirectoryStrategy = new ManageDirectoryStrategy();
                return await manageDirectoryStrategy.getListMainDirectories();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{base64path}")]
        public async Task<ActionResult<List<CustomDirectory>>> GetSubDirectories(string base64path)
        {
            try
            {
                var base64Bytes = System.Convert.FromBase64String(base64path);
                var path = System.Text.Encoding.UTF8.GetString(base64Bytes);
                var manageDirectoryStrategy = new ManageDirectoryStrategy();
                return await manageDirectoryStrategy.getSubDirectories(path);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
