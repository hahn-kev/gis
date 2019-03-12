using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Authorization;
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
        [Authorize(Policy = "people")]
        public IList<Person> ListSchoolAids()
        {
            return _personService.SchoolAids();
        }

        [HttpGet("school-aids/summaries")]
        [Authorize(Policy = "people")]
        public List<PersonWithRoleSummaries> ListSchoolAidSummaries()
        {
            return _personService.GetSchoolAidSummaries();
        }

        [HttpGet("{id}")]
        [Authorize]
        public Task<ActionResult<PersonWithOthers>> Get(Guid id)
        {
            return TryExecute(MyPolicies.peopleEdit, id, () => _personService.GetById(id));
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
        public Task<ActionResult<PersonWithOthers>> Update([FromBody] PersonWithOthers person)
        {
            return TryExecute(MyPolicies.peopleEdit,
                person.Id,
                () =>
                {
                    _personService.Save(person);
                    return person;
                });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public Task<ActionResult> DeletePerson(Guid id)
        {
            return TryExecute(MyPolicies.peopleEdit,
                id,
                () =>
                {
                    _personService.DeletePerson(id);
                    return (ActionResult) Ok();
                });
        }

        [HttpPost("role")]
        public Task<ActionResult<PersonRole>> UpdateRole([FromBody] PersonRole role)
        {
            return TryExecute(MyPolicies.peopleEdit,
                role.PersonId,
                () =>
                {
                    _personService.Save(role);
                    return role;
                });
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

        [HttpGet("role")]
        [MyAuthorize(MyPolicies.isSupervisor)]
        public List<PersonRoleWithJob> SupervisorsRoles(bool canStartDuringRange,
            DateTime beginRange,
            DateTime endRange)
        {
            var groupId = User.SupervisorGroupId();
            if (!groupId.HasValue) return new List<PersonRoleWithJob>();
            return _personService.RolesForOrgGroup(canStartDuringRange, beginRange, endRange, groupId.Value);
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