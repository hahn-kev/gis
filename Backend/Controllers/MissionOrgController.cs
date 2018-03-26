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
        private IEntityService _entityService;
        private IDbConnection _dbConnection;

        public MissionOrgController(IEntityService entityService, IDbConnection dbConnection)
        {
            _entityService = entityService;
            _dbConnection = dbConnection;
        }

        [HttpGet]
        public IList<MissionOrg> List()
        {
            return _dbConnection.MissionOrgs.ToList();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,hr")]
        public MissionOrg GetOrg(Guid id)
        {
            return _dbConnection.MissionOrgs.SingleOrDefault(org => org.Id == id);
        }

        [HttpPost]
        [Authorize(Roles = "admin,hr")]
        public MissionOrg Save([FromBody] MissionOrg org)
        {
            _entityService.Save(org);
            return org;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<MissionOrg>(id);
            return Ok();
        }
    }
}