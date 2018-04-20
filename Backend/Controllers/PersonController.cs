using System;
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
//        [Authorize(Roles = "admin,hr")]
        public IList<Person> List()
        {
            return _personService.People();
        }

        [HttpGet("school-aids")]
        [Authorize(Roles = "admin,hr")]
        public IList<Person> ListSchoolAids()
        {
            return _personService.SchoolAids();
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "admin,hr")]
        public PersonWithOthers Get(Guid id)
        {
            return _personService.GetById(id);
        }

        [HttpPost("self")]
        public IActionResult UpdateSelf([FromBody] PersonWithStaff person)
        {
            if (User.PersonId() != person.Id)
            {
                throw new UnauthorizedAccessException("You're only allowed to modify your own details ");
            }

            _personService.Save(person);
            return Json(person);
        }

        [HttpPost]
        [Authorize(Roles = "admin,hr")]
        public IActionResult Update([FromBody] PersonWithStaff person)
        {
            _personService.Save(person);
            return Json(person);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult DeletePerson(Guid id)
        {
            _personService.DeletePerson(id);
            return Ok();
        }

        [HttpPost("role")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult UpdateRole([FromBody] PersonRole role)
        {
            _personService.Save(role);
            return Json(role);
        }


        [HttpDelete("role/{roleId}")]
        [Authorize(Roles = "admin,hr")]
        public IActionResult DeleteRole(Guid roleId)
        {
            _personService.DeleteRole(roleId);
            return Ok();
        }

        [HttpGet("role")]
        [Authorize(Roles = "admin,hr")]
        public IList<PersonRoleWithJob> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personService.Roles(canStartDuringRange, beginRange, endRange);
        }

        [HttpGet("staff")]
        [Authorize(Roles = "admin,hr")]
        public IList<StaffWithName> Staff()
        {
            return _personService.StaffWithNames;
        }

        [HttpGet("staff/all")]
        [Authorize(Roles = "admin,hr")]
        public IList<PersonWithStaff> StaffAll()
        {
            return _personService.StaffAll;
        }

        [HttpGet("staff/summaries")]
        [Authorize(Roles = "admin,hr")]
        public IList<PersonWithStaffSummaries> StaffSummaries()
        {
            return _personService.StaffSummaries;
        }

        [HttpGet("{personId}/emergency")]
        [Authorize(Roles = "admin,hr")]
        public IList<EmergencyContactExtended> GetEmergencyContacts(Guid personId)
        {
            return _personService.GetEmergencyContacts(personId);
        }

        [HttpPost("emergency")]
        [Authorize(Roles = "admin,hr")]
        public EmergencyContactExtended UpdateEmergencyContact([FromBody] EmergencyContactExtended emergencyContact)
        {
            _personService.Save(emergencyContact);
            return emergencyContact;
        }

        [HttpDelete("emergency/{id}")]
        [Authorize(Roles = "admin,hr")]
        public void DeleteEmergencyContact(Guid id)
        {
            _personService.DeleteEmergencyContact(id);
        }
    }
}