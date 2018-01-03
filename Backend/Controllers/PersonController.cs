﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "admin,hr")]
        public IList<Person> List()
        {
            return _personService.People();
        }

        [HttpGet("leave")]
        public IList<PersonWithDaysOfLeave> PeopleWithDaysOfLeave()
        {
            return _personService.PeopleWithDaysOfLeave(
                User.IsAdminOrHr()
                    ? (Guid?) null
                    : (User.PersonId() ??
                       throw new AuthenticationException("If user isn't admin or hr they must have a personId")));
        }

        [HttpGet("{id}")]
        public PersonWithOthers Get(Guid id)
        {
            return _personService.GetById(id);
        }

        [HttpPost]
        [Authorize(Roles = "admin,hr")]
        public IActionResult Update([FromBody] PersonWithStaff person)
        {
            _personService.Save(person);
            return Ok();
        }

        [HttpPost("role")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult UpdateRole([FromBody] PersonRole role)
        {
            _personService.Save(role);
            return Json(role);
        }

        [HttpGet("role")]
        [Authorize(Roles = "admin,hr")]
        public IList<PersonRoleExtended> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personService.Roles(canStartDuringRange, beginRange, endRange);
        }

        [HttpGet("staff")]
        [Authorize(Roles = "admin,hr")]
        public IList<StaffWithName> Staff()
        {
            return _personService.StaffWithNames;
        }
    }
}