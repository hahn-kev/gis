using System;
using System.Security.Authentication;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class SelfController : MyController
    {
        private PersonService _personService;
        private LeaveService _leaveService;

        public SelfController(PersonService personService, LeaveService leaveService)
        {
            _personService = personService;
            _leaveService = leaveService;
        }

        [HttpGet("{id?}")]
        public PersonWithOthers Get()
        {

            var personId = User.PersonId() ?? Guid.Empty;
            if (personId == Guid.Empty)
            {
                return new PersonWithOthers();
            }
            return _personService.GetById(personId);
        }
    }
}