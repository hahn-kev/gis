using System;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "admin,hr")]
    public class EvaluationController : MyController
    {
        private readonly IEntityService _entityService;
        private IDbConnection _dbConnection;

        public EvaluationController(IEntityService entityService, IDbConnection dbConnection)
        {
            _entityService = entityService;
            _dbConnection = dbConnection;
        }

        [HttpPost]
        public Evaluation Update([FromBody] Evaluation evaluation)
        {
            _entityService.Save(evaluation);
            return evaluation;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _entityService.Delete<Evaluation>(id);
            return Ok();
        }
    }
}