using System;
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
        public Education Update([FromBody] Education education)
        {
            _entityService.Save(education);
            return education;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<Education>(id);
            return Ok();
        }
    }
}