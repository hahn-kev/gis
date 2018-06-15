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
    public class EndorsementController : MyController
    {
        private readonly EndorsementService _endorsementService;

        public EndorsementController(EndorsementService endorsementService)
        {
            _endorsementService = endorsementService;
        }

        [HttpGet]
        public IList<Endorsement> List() => _endorsementService.Endorsements;

        [HttpGet("{id}")]
        public Endorsement GetById(Guid id) => _endorsementService.GetById(id);

        [HttpGet("staff/{personId}")]
        public IList<StaffEndorsementWithName> ListStaffEndorsements(Guid personId)
        {
            return _endorsementService.ListStaffEndorsements(personId);
        }

        [HttpGet("required/{jobId}")]
        public IList<RequiredEndorsementWithName> ListRequiredEndorsements(Guid jobId)
        {
            return _endorsementService.ListRequiredEndorsements(jobId);
        }

        [HttpPost]
        public Endorsement Save([FromBody] Endorsement endorsement)
        {
            _endorsementService.Save(endorsement);
            return endorsement;
        }

        [HttpPost("staff")]
        public StaffEndorsement Save([FromBody] StaffEndorsement staffEndorsement)
        {
            _endorsementService.Save(staffEndorsement);
            return staffEndorsement;
        }

        [HttpPost("required")]
        public RequiredEndorsement Save([FromBody] RequiredEndorsement requiredEndorsement)
        {
            _endorsementService.Save(requiredEndorsement);
            return requiredEndorsement;
        }

        [HttpDelete("{endorsementId}")]
        public IActionResult DeleteEndorsement(Guid endorsementId)
        {
            _endorsementService.DeleteEndorsement(endorsementId);
            return Ok();
        }

        [HttpDelete("staff/{staffEndorsementId}")]
        public IActionResult DeleteStaffEndorsement(Guid staffEndorsementId)
        {
            _endorsementService.DeleteStaffEndorsement(staffEndorsementId);
            return Ok();
        }

        [HttpDelete("required/{requiredEndorsementId}")]
        public IActionResult DeleteRequiredEndorsement(Guid requiredEndorsementId)
        {
            _endorsementService.DeleteRequiredEndorsement(requiredEndorsementId);
            return Ok();
        }
    }
}