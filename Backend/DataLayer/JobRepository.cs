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

        public IQueryable<Job> Job => _dbConnection.Job.OrderBy(job => job.Title);

        public IQueryable<JobWithOrgGroup> JobsWithOrgGroup =>
            from job in _dbConnection.Job
            from orgGroup in _dbConnection.OrgGroups.LeftJoin(g => g.Id == job.OrgGroupId).DefaultIfEmpty()
            from grade in JobGrades.LeftJoin(grade => grade.Id == job.GradeId).DefaultIfEmpty()
            select new JobWithOrgGroup
            {
                Current = job.Current,
                Id = job.Id,
                GradeId = job.GradeId,
                OrgGroupId = job.OrgGroupId,
                JobDescription = job.JobDescription,
                Title = job.Title,
                Positions = job.Positions,
                Type = job.Type,
                Status = job.Status,
                OrgGroup = orgGroup,
                GradeNo = (int?) grade.GradeNo
            };

        public IQueryable<Grade> JobGrades => _dbConnection.JobGrades;

        public IQueryable<JobWithFilledInfo> JobWithFilledInfos =>
            from job in Job
            from role in _dbConnection.PersonRoles.LeftJoin(role => role.JobId == job.Id).DefaultIfEmpty()
            from grade in JobGrades.LeftJoin(grade => grade.Id == job.GradeId).DefaultIfEmpty()
            from org in _dbConnection.OrgGroups.LeftJoin(org => org.Id == job.OrgGroupId)
            group role by new {role.JobId, job, grade, org}
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
                Status = g.Key.job.Status,
                Type = g.Key.job.Type,
                Filled = g.Sum(role => role.Active ? 1 : 0),
                GradeNo = (int?) g.Key.grade.GradeNo,
                OrgGroupName = g.Key.org.GroupName
            };

        public JobWithRoles GetById(Guid jobId)
        {
            var job = JobsWithRoles.Single(j => j.Id == jobId);
            if (job != null)
            {
                job.Roles = PersonRolesExtended.Where(role => role.JobId == jobId).ToList();
                job.RequiredEndorsements = _requiredEndorsementsWithName.Where(re => re.JobId == jobId).ToList();
            }

            return job;
        }

        private IQueryable<JobWithRoles> JobsWithRoles =>
            from job in Job
            select new JobWithRoles
            {
                Id = job.Id,
                Current = job.Current,
                JobDescription = job.JobDescription,
                GradeId = job.GradeId,
                OrgGroupId = job.OrgGroupId,
                Positions = job.Positions,
                Title = job.Title,
                Status = job.Status,
                Type = job.Type
            };

        private IQueryable<RequiredEndorsementWithName> _requiredEndorsementsWithName =>
            (from requiredEndorsement in _dbConnection.RequiredEndorsements
                from endorsment in _dbConnection.Endorsements.LeftJoin(endorsement =>
                    endorsement.Id == requiredEndorsement.EndorsementId)
                select new RequiredEndorsementWithName
                {
                    Id = requiredEndorsement.Id,
                    JobId = requiredEndorsement.JobId,
                    EndorsementId = requiredEndorsement.EndorsementId,
                    EndorsementName = endorsment.Name
                });


        public IQueryable<PersonRoleExtended> PersonRolesExtended =>
            (from personRole in _dbConnection.PersonRoles
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
                    Notes = personRole.Notes,
                    PreferredName = person.PreferredName,
                    LastName = person.LastName
                }).OrderByDescending(extended => extended.StartDate);
    }
}