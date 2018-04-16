using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class EndorsmentRepository
    {
        private readonly IDbConnection _dbConnection;

        public EndorsmentRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Endorsment> Endorsments => _dbConnection.Endorsments;

        public IQueryable<StaffEndorsment> StaffEndorsments => _dbConnection.StaffEndorsments;

        public IQueryable<RequiredEndorsment> RequiredEndorsments => _dbConnection.RequiredEndorsments;

        public IQueryable<StaffEndorsmentWithName> StaffEndorsmentsWithName => from se in StaffEndorsments
            from e in Endorsments.LeftJoin(end => end.Id == se.EndorsmentId)
            select new StaffEndorsmentWithName
            {
                Id = se.Id,
                PersonId = se.PersonId,
                EndorsmentId = se.EndorsmentId,
                EndorsmentDate = se.EndorsmentDate,
                EndorsmentName = e.Name,
                EndorsmentAgency = e.Agency
            };

        public IQueryable<RequiredEndorsmentWithName> RequiredEndorsmentsWithName => from re in RequiredEndorsments
            from e in Endorsments.LeftJoin(e => e.Id == re.EndorsmentId)
            select new RequiredEndorsmentWithName
            {
                Id = re.Id,
                EndorsmentId = re.EndorsmentId,
                JobId = re.JobId,
                EndorsmentName = e.Name,
                EndorsmentAgency = e.Agency
            };
    }
}