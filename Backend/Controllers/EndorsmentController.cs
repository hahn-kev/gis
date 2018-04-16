using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,hr")]
    public class EndorsmentController : MyController
    {
        private readonly EndorsmentService _endorsmentService;

        public EndorsmentController(EndorsmentService endorsmentService)
        {
            _endorsmentService = endorsmentService;
        }

        [HttpGet]
        public IList<Endorsment> List() => _endorsmentService.Endorsments;

        [HttpGet("staff/{personId}")]
        public IList<StaffEndorsmentWithName> ListStaffEndorsments(Guid personId)
        {
            return _endorsmentService.ListStaffEndorsments(personId);
        }

        [HttpGet("required/{jobId}")]
        public IList<RequiredEndorsmentWithName> ListRequiredEndorsments(Guid jobId)
        {
            return _endorsmentService.ListRequiredEndorsments(jobId);
        }

        [HttpPost]
        public Endorsment Save([FromBody] Endorsment endorsment)
        {
            _endorsmentService.Save(endorsment);
            return endorsment;
        }

        [HttpPost("staff")]
        public StaffEndorsment Save([FromBody] StaffEndorsment staffEndorsment)
        {
            _endorsmentService.Save(staffEndorsment);
            return staffEndorsment;
        }

        [HttpPost("required")]
        public RequiredEndorsment Save([FromBody] RequiredEndorsment requiredEndorsment)
        {
            _endorsmentService.Save(requiredEndorsment);
            return requiredEndorsment;
        }

        [HttpDelete("{endorsmentId}")]
        public IActionResult DeleteEndorsment(Guid endorsmentId)
        {
            _endorsmentService.DeleteEndorsment(endorsmentId);
            return Ok();
        }

        [HttpDelete("staff/{staffEndorsmentId}")]
        public IActionResult DeleteStaffEndorsment(Guid staffEndorsmentId)
        {
            _endorsmentService.DeleteStaffEndorsment(staffEndorsmentId);
            return Ok();
        }

        [HttpDelete("required/{requiredEndorsmentId}")]
        public IActionResult DeleteRequiredEndorsment(Guid requiredEndorsmentId)
        {
            _endorsmentService.DeleteRequiredEndorsment(requiredEndorsmentId);
            return Ok();
        }
    }
}