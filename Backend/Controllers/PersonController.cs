using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly IAuthorizationService _authorizationService;

        public PersonController(PersonService personService, IAuthorizationService authorizationService)
        {
            _personService = personService;
            _authorizationService = authorizationService;
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
        [Authorize]
        public async Task<ActionResult<PersonWithOthers>> Get(Guid id)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, id, "peopleEdit");
            if (authorizationResult.Succeeded)
                return _personService.GetById(id);
            return new ForbidResult();
        }

        [HttpPost("self")]
        public IActionResult UpdateSelf([FromBody] PersonWithOthers person)
        {
            if (User.PersonId() != person.Id)
            {
                throw new UnauthorizedAccessException("You're only allowed to modify your own details ");
            }

            _personService.Save(person);
            return Json(person);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<PersonWithOthers>> Update([FromBody] PersonWithOthers person)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, person.Id, "peopleEdit");
            if (authorizationResult.Succeeded)
            {
                _personService.Save(person);
                return person;
            }

            return new ForbidResult();
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeletePerson(Guid id)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, id, "peopleEdit");
            if (authorizationResult.Succeeded)
            {
                _personService.DeletePerson(id);
                return Ok();
            }

            return new ForbidResult();
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
            return _personService.StaffWithNames.RemoveSalary();
        }

        [HttpGet("staff/all")]
        [Authorize(Policy = "staff")]
        public IList<PersonWithStaff> StaffAll()
        {
            return _personService.StaffAll.RemoveSalaryStaff();
        }

        [HttpGet("staff/summaries")]
        [Authorize(Policy = "staff")]
        public IList<PersonWithStaffSummaries> StaffSummaries()
        {
            return _personService.StaffSummaries.RemoveSalaryStaff();
        }

        [HttpGet("staff/roles")]
        [Authorize(Policy = "staff")]
        public IList<StaffWithRoles> StaffWithRoles()
        {
            var staff = _personService.StaffWithRoles;
            foreach (var s in staff)
            {
                s.StaffWithName?.RemoveSalary();
            }

            return staff;
        }

        [HttpGet("{personId}/emergency")]
        [Authorize(Policy = "contact")]
        public IList<EmergencyContactExtended> GetEmergencyContacts(Guid personId)
        {
            return _personService.GetEmergencyContacts(personId);
        }

        [HttpPost("emergency")]
        public EmergencyContactExtended UpdateEmergencyContact([FromBody] EmergencyContactExtended emergencyContact)
        {
            _personService.Save(emergencyContact);
            return emergencyContact;
        }

        [HttpDelete("emergency/{id}")]
        public void DeleteEmergencyContact(Guid id)
        {
            _personService.DeleteEmergencyContact(id);
        }
    }
}