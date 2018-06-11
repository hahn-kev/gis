using System;
using System.Collections.Generic;
using System.Linq;
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
        [Authorize(Policy = "orgGroup")]
        public OrgGroup Get(Guid id)
        {
            return _orgGroupService.GetById(id);
        }

        [HttpGet]
        public List<OrgGroupWithSupervisor> OrgGroups() => _orgGroupService.OrgGroups;

        [HttpPost]
        [Authorize(Policy = "orgGroup")]
        public OrgGroup Save([FromBody] OrgGroup orgGroup)
        {
            _orgGroupService.Save(orgGroup);
            return orgGroup;
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "orgGroup")]
        public IActionResult Delete(Guid id)
        {
            _orgGroupService.Delete(id);
            return Ok();
        }

        [HttpGet("orgTreeData/{groupId}")]
        [HttpGet("orgTreeData")]
        [Authorize("orgTreeData")]
        public OrgTreeData OrgTreeData(Guid? groupId = null)
        {
            return _orgGroupService.OrgTreeData(groupId);
        }
    }
}