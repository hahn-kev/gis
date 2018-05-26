using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class EvaluationRepository
    {
        private readonly IDbConnection _dbConnection;

        public EvaluationRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Evaluation> Evaluations => _dbConnection.Evaluations;

        public IQueryable<EvaluationWithNames> EvaluationWithNames =>
            (from eval in _dbConnection.Evaluations
                from role in _dbConnection.PersonRoles.LeftJoin(role => role.Id == eval.RoleId)
                from job in _dbConnection.Job.LeftJoin(job => job.Id == role.JobId)
                from evaluator in _dbConnection.People.LeftJoin(person => person.Id == eval.Evaluator)
                select new EvaluationWithNames
                {
                    Id = eval.Id,
                    PersonId = eval.PersonId,
                    Evaluator = eval.Evaluator,
                    RoleId = eval.RoleId,
                    Date = eval.Date,
                    Notes = eval.Notes,
                    Result = eval.Result,
                    Score = eval.Score,
                    Total = eval.Total,
                    JobTitle = job.Title,
                    EvaluatorPreferredName = evaluator.PreferredName ?? evaluator.FirstName,
                    EvaluatorLastName = evaluator.LastName
                }).OrderByDescending(eval => eval.Date);
    }
}