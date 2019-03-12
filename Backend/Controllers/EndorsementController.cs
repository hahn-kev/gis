using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Authorization;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
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
        public Task<ActionResult<List<StaffEndorsementWithName>>> ListStaffEndorsements(Guid personId)
        {
            return TryExecute(MyPolicies.peopleEdit,
                personId,
                () => _endorsementService.ListStaffEndorsements(personId));
        }

        [HttpGet("required/{jobId}")]
        public IList<RequiredEndorsementWithName> ListRequiredEndorsements(Guid jobId)
        {
            return _endorsementService.ListRequiredEndorsements(jobId);
        }

        [HttpPost]
        [MyAuthorize(MyPolicies.endorsement)]
        public Endorsement Save([FromBody] Endorsement endorsement)
        {
            _endorsementService.Save(endorsement);
            return endorsement;
        }

        [HttpPost("staff")]
        public Task<ActionResult<StaffEndorsement>> Save([FromBody] StaffEndorsement staffEndorsement)
        {
            return TryExecute(MyPolicies.peopleEdit,
                staffEndorsement.PersonId,
                () =>
                {
                    _endorsementService.Save(staffEndorsement);
                    return staffEndorsement;
                });
        }

        [HttpPost("required")]
        public RequiredEndorsement Save([FromBody] RequiredEndorsement requiredEndorsement)
        {
            _endorsementService.Save(requiredEndorsement);
            return requiredEndorsement;
        }

        [HttpDelete("{endorsementId}")]
        [MyAuthorize(MyPolicies.endorsement)]
        public IActionResult DeleteEndorsement(Guid endorsementId)
        {
            _endorsementService.DeleteEndorsement(endorsementId);
            return Ok();
        }

        [HttpDelete("staff/{staffEndorsementId}")]
        [MyAuthorize(MyPolicies.hrSupervisorAdmin)]
        public IActionResult DeleteStaffEndorsement(Guid staffEndorsementId)
        {
            //todo prevent access to only valid supervisors
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