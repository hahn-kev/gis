using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public class EvaluationService
    {
        private readonly IEntityService _entityService;
        private readonly PersonRepository _personRepository;
        private readonly EvaluationRepository _evaluationRepository;

        public EvaluationService(IEntityService entityService,
            PersonRepository personRepository,
            EvaluationRepository evaluationRepository)
        {
            _entityService = entityService;
            _personRepository = personRepository;
            _evaluationRepository = evaluationRepository;
        }

        public void Save(Evaluation evaluation)
        {
            _entityService.Save(evaluation);
        }

        public void DeleteEvaluation(Guid id)
        {
            _entityService.Delete<Evaluation>(id);
        }

        public IList<EvaluationWithNames> ByPersonId(Guid personId)
        {
            return _evaluationRepository.EvaluationWithNames
                .Where(evaluation => evaluation.PersonId == personId)
                .ToList();
        }

        public IList<PersonEvaluationSummary> Summaries()
        {
            return (from person in _personRepository.PeopleWithStaff
                from evals in _evaluationRepository.Evaluations.InnerJoin(e => e.PersonId == person.Id)
                group evals by new {person, evals.PersonId}
                into e
                select new PersonEvaluationSummary
                {
                    Person = e.Key.person,
                    Evaluations = e.Count(),
                    ExcellentEvaluations = e.Sum(evaluation => evaluation.Result == EvaluationResult.Excellent ? 1 : 0),
                    GoodEvaluations = e.Sum(evaluation => evaluation.Result == EvaluationResult.Good ? 1 : 0),
                    PoorEvaluations = e.Sum(evaluation => evaluation.Result == EvaluationResult.Poor ? 1 : 0),
                    AveragePercentage = e.Average(evaluation => evaluation.Score / evaluation.Total * 100)
                }).ToList();
        }
    }
}