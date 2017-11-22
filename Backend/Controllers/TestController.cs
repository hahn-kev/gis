using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    #if DEBUG
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return LocalRedirect("~/message?text=hello_world");
        }

        [HttpGet("throw")]
        public IActionResult Throw()
        {
            throw new Exception("some error man!");
        }
    }
    #endif
}