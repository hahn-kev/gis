using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Entities;
using LinqToDB;
using LinqToDB.Data;

namespace Backend.DataLayer
{
    public class TrainingRepository
    {
        private readonly IDbConnection _connection;

        public TrainingRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public IQueryable<TrainingRequirement> TrainingRequirements => _connection.TrainingRequirements;
        public IQueryable<StaffTraining> StaffTraining => _connection.StaffTraining;

        public IQueryable<StaffTrainingWithRequirement> StaffTrainingWithRequirements =>
            from training in StaffTraining
            from requirement in TrainingRequirements.LeftJoin(requirement =>
                requirement.Id == training.TrainingRequirementId)
            select new StaffTrainingWithRequirement
            {
                Id = training.Id,
                CompletedDate = training.CompletedDate,
                RequirementName = requirement.Name,
                RequirementScope = requirement.Scope,
                StaffId = training.StaffId,
                TrainingRequirementId = training.TrainingRequirementId
            };

        public void InsertAll(IEnumerable<StaffTraining> staffTraining)
        {
            _connection.BulkCopy(staffTraining);
        }
    }
}