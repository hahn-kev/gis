using System.Linq;
using Backend.Entities;
using LinqToDB;

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
    }
}