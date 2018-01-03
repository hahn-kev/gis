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
    public class OrgGroupController : MyController
    {
        private readonly OrgGroupService _orgGroupService;
        private readonly IEntityService _entityService;

        public OrgGroupController(OrgGroupService orgGroupService, IEntityService entityService)
        {
            _orgGroupService = orgGroupService;
            _entityService = entityService;
        }

        [HttpGet("{id}")]
        public OrgGroup Get(Guid id)
        {
            return _orgGroupService.GetById(id);
        }

        [HttpGet]
        public List<OrgGroup> OrgGroups() => _orgGroupService.OrgGroups;

        [HttpPost]
        public OrgGroup Save([FromBody] OrgGroup orgGroup)
        {
            _entityService.Save(orgGroup);
            return orgGroup;
        }
    }
}