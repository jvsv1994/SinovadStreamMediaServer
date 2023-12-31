﻿using Microsoft.AspNetCore.Mvc;
using SinovadMediaServer.Application.DTOs;
using SinovadMediaServer.Strategies;

namespace SinovadMediaServer.Controllers
{
    [Route("api/directories")]
    public class DirectoryController : Controller
    {

        [HttpGet]
        public async Task<ActionResult<List<DirectoryDto>>> GetMainDirectories()
        {
            try
            {
                var manageDirectoryStrategy = new ManageDirectoryStrategy();
                return await manageDirectoryStrategy.GetListMainDirectories();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        [HttpGet("{base64path}")]
        public async Task<ActionResult<List<DirectoryDto>>> GetSubDirectories(string base64path)
        {
            try
            {
                var base64Bytes = Convert.FromBase64String(base64path);
                var path = System.Text.Encoding.UTF8.GetString(base64Bytes);
                var manageDirectoryStrategy = new ManageDirectoryStrategy();
                return await manageDirectoryStrategy.GetSubDirectories(path);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

    }
}
