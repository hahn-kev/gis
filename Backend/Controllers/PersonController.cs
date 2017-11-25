using System;
using System.Collections.Generic;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class PersonController : Controller
    {
        private readonly PersonService _personService;
        private readonly EntityService _entityService;

        public PersonController(PersonService personService, EntityService entityService)
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
    }
}