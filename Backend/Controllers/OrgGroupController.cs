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

        public OrgGroupController(OrgGroupService orgGroupService)
        {
            _orgGroupService = orgGroupService;
        }

        [HttpGet("{id}")]
        public OrgGroup Get(Guid id)
        {
            return _orgGroupService.GetById(id);
        }

        [HttpGet]
        public List<OrgGroup> OrgGroups() => _orgGroupService.OrgGroups;
    }
}