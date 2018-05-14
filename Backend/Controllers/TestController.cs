using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Backend.Controllers
{
    #if DEBUG
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class TestController : MyController
    {
        private ILogger _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

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
        
        public class TestClass
        {
            public DateTime Date { get; set; }
        }

        [HttpPost]
        public TestClass Test([FromBody] TestClass value)
        {
            _logger.LogDebug("Date: {0}", value.Date);
            return value;
        }
    }
    #endif
}