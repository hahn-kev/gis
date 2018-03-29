using System;
using System.Collections.Generic;
using System.Linq;
using Backend.DataLayer;
using Backend.Entities;
using LinqToDB;

namespace Backend.Services
{
    public class TrainingService
    {
        private readonly TrainingRepository _trainingRepository;
        private readonly IEntityService _entityService;

        public TrainingService(TrainingRepository trainingRepository, IEntityService entityService)
        {
            _trainingRepository = trainingRepository;
            _entityService = entityService;
        }

        public IList<TrainingRequirement> TrainingRequirements => _trainingRepository.TrainingRequirements.ToList();
        private IQueryable<StaffTraining> StaffTraining => _trainingRepository.StaffTraining;

        public IList<StaffTraining> GetByYear(int year)
        {
            return _trainingRepository.GetStaffTrainingByYearMonth(year, DateTime.Now.Month).ToList();
        }

        public IList<StaffTrainingWithRequirement> GetByStaff(Guid staffId)
        {
            return _trainingRepository.StaffTrainingWithRequirements.Where(
                requirement => requirement.StaffId == staffId).ToList();
        }

        public TrainingRequirement GetById(Guid id) =>
            _trainingRepository.TrainingRequirements.FirstOrDefault(requirement => requirement.Id == id);


        public void Save(TrainingRequirement entity)
        {
            _entityService.Save(entity);
        }

        public void Save(StaffTraining staffTraining)
        {
            _entityService.Save(staffTraining);
        }

        public void DeleteRequirement(Guid id)
        {
            _trainingRepository.DeleteRequirement(id);
        }

        public void DeleteStaffTraining(Guid id)
        {
            _entityService.Delete<StaffTraining>(id);
        }

        public void MarkAllComplete(List<Guid> staffIds, Guid requirementId, DateTime completeDate)
        {
            var existingTrainings = StaffTraining.Where(training =>
                training.TrainingRequirementId == requirementId && staffIds.Contains(training.StaffId) &&
                training.CompletedDate.Value.InSchoolYear(completeDate.SchoolYear())).ToList();
            if (existingTrainings.Any())
            {
                staffIds.RemoveAll(id => existingTrainings.Any(training => training.StaffId == id));
            }

            _trainingRepository.InsertAll(staffIds.Select(staffId => new StaffTraining
            {
                Id = Guid.NewGuid(),
                CompletedDate = completeDate,
                StaffId = staffId,
                TrainingRequirementId = requirementId
            }));
        }
    }
}