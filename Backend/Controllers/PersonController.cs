using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class PersonController : MyController
    {
        private readonly PersonService _personService;
        private readonly IEntityService _entityService;

        public PersonController(PersonService personService, IEntityService entityService)
        {
            _personService = personService;
            _entityService = entityService;
        }

        [HttpGet]
        public IList<Person> List()
        {
            return _personService.People();
        }

        [HttpGet("{id}")]
        public PersonExtended Get(Guid id)
        {
            return _personService.GetById(id);
        }

        [HttpPost]
        public IActionResult Update([FromBody] PersonExtended person)
        {
            _entityService.Save(person);
            return Ok();
        }

        [HttpPost("role")]
        public IActionResult UpdateRole([FromBody] PersonRole role)
        {
            _personService.Save(role);
            return Json(role);
        }

        [HttpGet("role")]
        public IList<PersonRoleExtended> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personService.Roles(canStartDuringRange, beginRange, endRange);
        }
    }
}