using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class LeaveRequestController : Controller
    {
        private readonly LeaveRequestService _leaveRequestService;

        public LeaveRequestController(LeaveRequestService leaveRequestService)
        {
            _leaveRequestService = leaveRequestService;
        }

        [HttpPost]
        public async Task<IActionResult> RequestLeave([FromBody] LeaveRequest leaveRequest)
        {
            if (ModelState.ErrorCount > 0)
            {
                throw new Exception(string.Join(", ",
                    ModelState.Values.Where(entry => entry.Errors.Count > 0).SelectMany(entry => entry.Errors)
                        .Select(error => error.Exception.Message)));
            }

            Person notified = await _leaveRequestService.RequestLeave(leaveRequest);
            return Json(notified);
        }

        [HttpGet("approve")]
        public IActionResult Approve(Guid leaveRequestId)
        {
            return Ok();
        }
    }
}