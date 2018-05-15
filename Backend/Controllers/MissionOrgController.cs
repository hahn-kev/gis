using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class MissionOrgController : MyController
    {
        private readonly IEntityService _entityService;
        private readonly MissionOrgRepository _orgRepository;

        public MissionOrgController(IEntityService entityService, MissionOrgRepository orgRepository)
        {
            _entityService = entityService;
            _orgRepository = orgRepository;
        }

        [HttpGet]
        public IList<MissionOrgWithNames> List()
        {
            return _orgRepository.MissionOrgsWithNames.OrderBy(org => org.Name).ToList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,hr,registrar")]
        public MissionOrgWithYearSummaries GetOrg(Guid id)
        {
            return _orgRepository.GetOrg(id);
        }

        [HttpPost]
        [Authorize(Roles = "admin,hr,registrar")]
        public MissionOrg Save([FromBody] MissionOrg org)
        {
            _entityService.Save(org);
            return org;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,hr,registrar")]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<MissionOrg>(id);
            return Ok();
        }

        [HttpPost("year")]
        [Authorize(Roles = "admin,hr,registrar")]
        public MissionOrgYearSummary Save([FromBody] MissionOrgYearSummary yearSummary)
        {
            _entityService.Save(yearSummary);
            return yearSummary;
        }

        [HttpDelete("year/{id}")]
        [Authorize(Roles = "admin,hr,registrar")]
        public IActionResult DeleteYear(Guid id)
        {
            _entityService.Delete<MissionOrgYearSummary>(id);
            return Ok();
        }
    }
}