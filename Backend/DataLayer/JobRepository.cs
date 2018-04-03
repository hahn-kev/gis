using System;
using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class JobRepository
    {
        private readonly IDbConnection _dbConnection;

        public JobRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Job> Job => _dbConnection.Job;
        public IQueryable<Grade> JobGrades => _dbConnection.JobGrades;

        public IQueryable<JobWithFilledInfo> JobWithFilledInfos =>
            from job in Job
            from role in _dbConnection.PersonRoles.LeftJoin(role => role.JobId == job.Id)
            where role.Active
            group role by new {role.JobId, job}
            into g
            select new JobWithFilledInfo
            {
                Id = g.Key.job.Id,
                Title = g.Key.job.Title,
                Current = g.Key.job.Current,
                GradeId = g.Key.job.GradeId,
                JobDescription = g.Key.job.JobDescription,
                OrgGroupId = g.Key.job.OrgGroupId,
                Positions = g.Key.job.Positions,
                Type = g.Key.job.Type,
                Filled = g.Count()
            };

        public JobWithRoles GetById(Guid jobId)
        {
            var job = JobsWithRoles.SingleOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                job.Roles = PersonRolesExtended.Where(role => role.JobId == jobId).ToList();
            }

            return job;
        }

        private IQueryable<JobWithRoles> JobsWithRoles => from job in Job
            select new JobWithRoles
            {
                Id = job.Id,
                Current = job.Current,
                JobDescription = job.JobDescription,
                GradeId = job.GradeId,
                OrgGroupId = job.OrgGroupId,
                Positions = job.Positions,
                Title = job.Title,
                Type = job.Type
            };


        public IQueryable<PersonRoleExtended> PersonRolesExtended =>
            from personRole in _dbConnection.PersonRoles
            join person in _dbConnection.People on personRole.PersonId equals person.Id
            join job in _dbConnection.Job on personRole.JobId equals job.Id
            select new PersonRoleExtended
            {
                Id = personRole.Id,
                JobId = personRole.JobId,
                PersonId = personRole.PersonId,
                Active = personRole.Active,
                StartDate = personRole.StartDate,
                EndDate = personRole.EndDate,
                PreferredName = person.PreferredName,
                LastName = person.LastName
            };
    }
}