using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class OrgGroupController : Controller
    {
        private readonly OrgGroupService _orgGroupService;
        private readonly EntityService _entityService;

        public OrgGroupController(OrgGroupService orgGroupService, EntityService entityService)
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