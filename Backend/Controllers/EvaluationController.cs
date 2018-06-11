using System;
using System.Collections.Generic;
using Backend.DataLayer;
using Backend.Entities;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Policy = "evaluations")]
    public class EvaluationController : MyController
    {
        private readonly EvaluationService _evaluationService;

        public EvaluationController(EvaluationService evaluationService)
        {
            _evaluationService = evaluationService;
        }

        [HttpPost]
        public Evaluation Update([FromBody] Evaluation evaluation)
        {
            _evaluationService.Save(evaluation);
            return evaluation;
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            _evaluationService.DeleteEvaluation(id);
            return Ok();
        }

        [HttpGet("summaries")]
        public IList<PersonEvaluationSummary> Summaries()
        {
            return _evaluationService.Summaries();
        }

        [HttpGet("person/{id}")]
        public IList<EvaluationWithNames> ByPersonId(Guid id)
        {
            return _evaluationService.ByPersonId(id);
        }
    }
}