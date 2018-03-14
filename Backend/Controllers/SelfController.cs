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
        public IActionResult Get(Guid? id = null)
        {
            if (!User.IsAdminOrHr() && id.HasValue)
            {
                throw new AuthenticationException("Non admin/hr user's arent allowed to view other users 'Self' page");
            }

            var personId = id ?? User.PersonId() ?? Guid.Empty;
            if (personId == Guid.Empty)
            {
                return Json(new Self());
            }
            var personWithOthers =
                _personService.GetById(personId);
            return Json(new Self
            {
                Person = personWithOthers,
                LeaveDetails = _leaveService.GetCurrentLeaveDetails(personWithOthers)
            });
        }
    }
}