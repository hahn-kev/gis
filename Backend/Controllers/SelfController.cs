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
            var personWithOthers =
                _personService.GetById(id ?? User.PersonId() ??
                                       throw new NullReferenceException("logged in user doesn't have a personId"));
            return Json(new Self
            {
                Person = personWithOthers,
                LeaveDetails = _leaveService.GetCurrentLeaveDetails(personWithOthers)
            });
        }
    }
}