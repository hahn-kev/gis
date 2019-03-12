using System;
using System.Threading.Tasks;
using Backend.Authorization;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class EducationController : MyController
    {
        private readonly IEntityService _entityService;

        public EducationController(IEntityService entityService)
        {
            _entityService = entityService;
        }

        [HttpPost]
        public Task<ActionResult<Education>> Update([FromBody] Education education)
        {
            return TryExecute(MyPolicies.peopleEdit,
                education.PersonId,
                () =>
                {
                    _entityService.Save(education);
                    return education;
                });
        }

        [HttpDelete("{id}")]
        [MyAuthorize(MyPolicies.hrSupervisorAdmin)]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<Education>(id);
            return Ok();
        }
    }
}