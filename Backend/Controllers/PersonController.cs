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

        public PersonController(PersonService personService)
        {
            _personService = personService;
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
            _personService.Update(person);
            return Ok();
        }
    }
}