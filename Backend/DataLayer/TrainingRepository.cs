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
        private readonly DbConnection _connection;

        public TrainingRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public IQueryable<TrainingRequirement> TrainingRequirements => _connection.TrainingRequirements;
        public IQueryable<StaffTraining> StaffTraining => _connection.StaffTraining;

        public void InsertAll(IEnumerable<StaffTraining> staffTraining)
        {
            _connection.BulkCopy(staffTraining);
        }
    }
}