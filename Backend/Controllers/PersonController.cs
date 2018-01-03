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

        public PersonController(PersonService personService)
        {
            _personService = personService;
        }

        [HttpGet]
        public IList<Person> List()
        {
            return _personService.People();
        }

        [HttpGet("leave")]
        public IList<PersonWithDaysOfLeave> PeopleWithDaysOfLeave()
        {
            return _personService.PeopleWithDaysOfLeave(
                User.IsAdminOrHr() ? null : User.PersonId());
        }

        [HttpGet("{id}")]
        public PersonWithOthers Get(Guid id)
        {
            return _personService.GetById(id);
        }

        [HttpPost]
        public IActionResult Update([FromBody] PersonWithStaff person)
        {
            _personService.Save(person);
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

        [HttpGet("staff")]
        public IList<StaffWithName> Staff()
        {
            return _personService.StaffWithNames;
        }
    }
}