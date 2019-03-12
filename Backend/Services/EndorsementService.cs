using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class EndorsementService
    {
        private readonly EndorsementRepository _endorsementRepository;
        private readonly IEntityService _entityService;

        public EndorsementService(EndorsementRepository endorsementRepository, IEntityService entityService)
        {
            _endorsementRepository = endorsementRepository;
            _entityService = entityService;
        }

        public IList<Endorsement> Endorsements => _endorsementRepository.Endorsements.ToList();

        public Endorsement GetById(Guid id) =>
            _endorsementRepository.Endorsements.Single(endorsement => endorsement.Id == id);

        public List<StaffEndorsementWithName> ListStaffEndorsements(Guid personId) =>
            _endorsementRepository.StaffEndorsementsWithName.Where(se => se.PersonId == personId).ToList();

        public IList<RequiredEndorsementWithName> ListRequiredEndorsements(Guid jobId) =>
            _endorsementRepository.RequiredEndorsementsWithName.Where(re => re.JobId == jobId).ToList();

        public void Save(Endorsement endorsement)
        {
            _entityService.Save(endorsement);
        }

        public void Save(StaffEndorsement staffEndorsement)
        {
            _entityService.Save(staffEndorsement);
        }

        public void Save(RequiredEndorsement requiredEndorsement)
        {
            _entityService.Save(requiredEndorsement);
        }

        public void DeleteEndorsement(Guid endorsementId)
        {
            _entityService.Delete<Endorsement>(endorsementId);
        }

        public void DeleteStaffEndorsement(Guid staffEndorsementId)
        {
            _entityService.Delete<StaffEndorsement>(staffEndorsementId);
        }

        public void DeleteRequiredEndorsement(Guid requiredEndorsementId)
        {
            _entityService.Delete<RequiredEndorsement>(requiredEndorsementId);
        }
    }
}