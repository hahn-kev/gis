﻿using System;
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

        public IQueryable<StaffTraining> GetStaffTrainingByYearMonth(int schoolYear, int schoolMonth)
        {
            return from training in StaffTraining
                from requirement in TrainingRequirements.InnerJoin(r => r.Id == training.TrainingRequirementId)
                where requirement.RenewMonthsCount == -1 || training.CompletedDate.InMonthBlock(schoolYear, schoolMonth, requirement.RenewMonthsCount)
                select training;
        }

        public void InsertAll(IEnumerable<StaffTraining> staffTraining)
        {
            _connection.BulkCopy(staffTraining);
        }

        public void DeleteRequirement(Guid id)
        {
            using (var transaction = _connection.BeginTransaction())
            {
                _connection.StaffTraining.Where(training => training.TrainingRequirementId == id).Delete();
                _connection.TrainingRequirements.Where(requirement => requirement.Id == id).Delete();
                transaction.Commit();
            }
        }
    }
}