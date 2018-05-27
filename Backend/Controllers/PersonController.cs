using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Authentication;
using Backend.Entities;
using Backend.Services;
using Backend.Utils;
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
        [Authorize(Policy = "people")]
        public IList<Person> ListSchoolAids()
        {
            return _personService.SchoolAids();
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "people")]
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
        [Authorize(Policy = "people")]
        public IActionResult Update([FromBody] PersonWithStaff person)
        {
            _personService.Save(person);
            return Json(person);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "people")]
        public IActionResult DeletePerson(Guid id)
        {
            _personService.DeletePerson(id);
            return Ok();
        }

        [HttpPost("role")]
        [Authorize(Policy = "role")]
        public IActionResult UpdateRole([FromBody] PersonRole role)
        {
            _personService.Save(role);
            return Json(role);
        }


        [HttpDelete("role/{roleId}")]
        [Authorize(Policy = "role")]
        public IActionResult DeleteRole(Guid roleId)
        {
            _personService.DeleteRole(roleId);
            return Ok();
        }

        [HttpGet("role")]
        [Authorize(Policy = "role")]
        public IList<PersonRoleWithJob> Roles(bool canStartDuringRange, DateTime beginRange, DateTime endRange)
        {
            return _personService.Roles(canStartDuringRange, beginRange, endRange);
        }

        [HttpGet("staff")]
        [Authorize(Policy = "staff")]
        public IList<StaffWithName> Staff()
        {
            return _personService.StaffWithNames.ForEach(s => s.RemoveSalary());
        }

        [HttpGet("staff/all")]
        [Authorize(Policy = "staff")]
        public IList<PersonWithStaff> StaffAll()
        {
            return _personService.StaffAll.ForEach(p => p?.Staff.RemoveSalary());
        }

        [HttpGet("staff/summaries")]
        [Authorize(Policy = "staff")]
        public IList<PersonWithStaffSummaries> StaffSummaries()
        {
            return _personService.StaffSummaries.ForEach(p => p?.Staff.RemoveSalary());
        }

        [HttpGet("{personId}/emergency")]
        [Authorize(Policy = "contact")]
        public IList<EmergencyContactExtended> GetEmergencyContacts(Guid personId)
        {
            return _personService.GetEmergencyContacts(personId);
        }

        [HttpPost("emergency")]
        [Authorize(Policy = "contact")]
        public EmergencyContactExtended UpdateEmergencyContact([FromBody] EmergencyContactExtended emergencyContact)
        {
            _personService.Save(emergencyContact);
            return emergencyContact;
        }

        [HttpDelete("emergency/{id}")]
        [Authorize(Policy = "contact")]
        public void DeleteEmergencyContact(Guid id)
        {
            _personService.DeleteEmergencyContact(id);
        }
    }
}