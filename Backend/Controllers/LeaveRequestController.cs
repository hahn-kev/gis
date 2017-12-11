﻿using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class LeaveRequestController : MyController
    {
        private readonly LeaveRequestService _leaveRequestService;
        private readonly ClaimsIdentityOptions _identityOptions;

        public LeaveRequestController(LeaveRequestService leaveRequestService, IOptions<ClaimsIdentityOptions> identityOptions)
        {
            _leaveRequestService = leaveRequestService;
            _identityOptions = identityOptions.Value;
        }

        [HttpPost]
        public async Task<IActionResult> RequestLeave([FromBody] LeaveRequest leaveRequest)
        {
            Person notified = await _leaveRequestService.RequestLeave(leaveRequest);
            return Json(notified);
        }

        [HttpGet("approve/{leaveRequestId}")]
        public IActionResult Approve(Guid leaveRequestId)
        {
            var userId = int.Parse(User.FindFirstValue(_identityOptions.UserIdClaimType));
            _leaveRequestService.ApproveLeaveRequest(leaveRequestId, userId);
            return this.ShowFrontendMessage("Leave request approved");
        }
    }
}