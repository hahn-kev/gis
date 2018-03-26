using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class OrgGroupController : MyController
    {
        private readonly OrgGroupService _orgGroupService;

        public OrgGroupController(OrgGroupService orgGroupService)
        {
            _orgGroupService = orgGroupService;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,hr")]
        public OrgGroup Get(Guid id)
        {
            return _orgGroupService.GetById(id);
        }

        [HttpGet]
        public List<OrgGroup> OrgGroups() => _orgGroupService.OrgGroups;

        [HttpPost]
        [Authorize(Roles = "admin,hr")]
        public OrgGroup Save([FromBody] OrgGroup orgGroup)
        {
            _orgGroupService.Save(orgGroup);
            return orgGroup;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult Delete(Guid id)
        {
            _orgGroupService.Delete(id);
            return Ok();
        }
    }
}