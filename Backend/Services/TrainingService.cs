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
        public IQueryable<StaffTraining> StaffTraining => _trainingRepository.StaffTraining;

        public TrainingRequirement GetById(Guid id) =>
            _trainingRepository.TrainingRequirements.FirstOrDefault(requirement => requirement.Id == id);


        public void Save(TrainingRequirement entity)
        {
            _entityService.Save(entity);
        }

        public void Delete(Guid id)
        {
            _entityService.Delete<TrainingRequirement>(id);
        }
    }
}