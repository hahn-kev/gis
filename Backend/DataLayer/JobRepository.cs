using System.Linq;
using Backend.Entities;

namespace Backend.DataLayer
{
    public class JobRepository
    {
        private readonly IDbConnection _dbConnection;

        public JobRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public IQueryable<Job> Job => _dbConnection.Job;
    }
}