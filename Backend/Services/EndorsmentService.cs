using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;

namespace Backend.Services
{
    public class EndorsmentService
    {
        private readonly EndorsmentRepository _endorsmentRepository;
        private readonly IEntityService _entityService;

        public EndorsmentService(EndorsmentRepository endorsmentRepository, IEntityService entityService)
        {
            _endorsmentRepository = endorsmentRepository;
            _entityService = entityService;
        }

        public IList<Endorsment> Endorsments => _endorsmentRepository.Endorsments.ToList();

        public IList<StaffEndorsmentWithName> ListStaffEndorsments(Guid personId) =>
            _endorsmentRepository.StaffEndorsmentsWithName.Where(se => se.PersonId == personId).ToList();

        public IList<RequiredEndorsmentWithName> ListRequiredEndorsments(Guid jobId) =>
            _endorsmentRepository.RequiredEndorsmentsWithName.Where(re => re.JobId == jobId).ToList();

        public void Save(Endorsment endorsment)
        {
            _entityService.Save(endorsment);
        }

        public void Save(StaffEndorsment staffEndorsment)
        {
            _entityService.Save(staffEndorsment);
        }

        public void Save(RequiredEndorsment requiredEndorsment)
        {
            _entityService.Save(requiredEndorsment);
        }

        public void DeleteEndorsment(Guid endorsmentId)
        {
            _entityService.Delete<Endorsment>(endorsmentId);
        }

        public void DeleteStaffEndorsment(Guid staffEndorsmentId)
        {
            _entityService.Delete<StaffEndorsment>(staffEndorsmentId);
        }

        public void DeleteRequiredEndorsment(Guid requiredEndorsmentId)
        {
            _entityService.Delete<RequiredEndorsment>(requiredEndorsmentId);
        }
    }
}