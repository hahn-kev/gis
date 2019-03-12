using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Backend.Authorization;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    public class EvaluationController : MyController
    {
        private readonly EvaluationService _evaluationService;

        public EvaluationController(EvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        [HttpPost]
        public Task<ActionResult<Evaluation>> Update([FromBody] Evaluation evaluation)
        {
            return TryExecute(MyPolicies.peopleEdit,
                evaluation.PersonId,
                () =>
                {
                    _evaluationService.Save(evaluation);
                    return evaluation;
                });
        }

        [HttpDelete("{id}")]
        [MyAuthorize(MyPolicies.hrSupervisorAdmin)]
        public IActionResult Delete(Guid id)
        {
            _evaluationService.DeleteEvaluation(id);
            return Ok();
        }

        [HttpGet("summaries")]
        [MyAuthorize(MyPolicies.evaluations)]
        public IList<PersonEvaluationSummary> Summaries()
        {
            return _evaluationService.Summaries();
        }

        [HttpGet("person/{id}")]
        [MyAuthorize(MyPolicies.evaluations)]
        public IList<EvaluationWithNames> ByPersonId(Guid id)
        {
            return _evaluationService.ByPersonId(id);
        }
    }
}