using System.Linq;
using Backend.Entities;

namespace Backend.DataLayer
{
    public class OrgGroupRepository
    {
        private readonly DbConnection _connection;

        public OrgGroupRepository(DbConnection connection)
        {
            _connection = connection;
        }

        public IQueryable<OrgGroup> OrgGroups => _connection.OrgGroups;
    }
}