using System.Linq;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class EndorsementRepository
    {
        private readonly IDbConnection _dbConnection;

        public EndorsementRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Endorsement> Endorsements => _dbConnection.Endorsements;

        public IQueryable<StaffEndorsement> StaffEndorsements => _dbConnection.StaffEndorsements;

        public IQueryable<RequiredEndorsement> RequiredEndorsements => _dbConnection.RequiredEndorsements;

        public IQueryable<StaffEndorsementWithName> StaffEndorsementsWithName => from se in StaffEndorsements
            from e in Endorsements.LeftJoin(end => end.Id == se.EndorsementId)
            select new StaffEndorsementWithName
            {
                Id = se.Id,
                PersonId = se.PersonId,
                EndorsementId = se.EndorsementId,
                EndorsementDate = se.EndorsementDate,
                EndorsementName = e.Name,
                EndorsementAgency = e.Agency
            };

        public IQueryable<RequiredEndorsementWithName> RequiredEndorsementsWithName => from re in RequiredEndorsements
            from e in Endorsements.LeftJoin(e => e.Id == re.EndorsementId)
            select new RequiredEndorsementWithName
            {
                Id = re.Id,
                EndorsementId = re.EndorsementId,
                JobId = re.JobId,
                EndorsementName = e.Name,
                EndorsementAgency = e.Agency
            };
    }
}