using System;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class DonationController : MyController
    {
        private readonly IEntityService _entityService;

        public DonationController(IEntityService entityService)
        {
            _entityService = entityService;
        }

        [HttpPost]
        public Donation Save([FromBody] Donation donation)
        {
            _entityService.Save(donation);
            return donation;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<Donation>(id);
            return Ok();
        }
    }
}