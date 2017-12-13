using System.Linq;
using Backend.Entities;
using LinqToDB;

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

        public IQueryable<OrgGroupWithSupervisor> OrgGroupsWithSupervisor =>
            from orgGroup in _connection.GetTable<OrgGroupWithSupervisor>()
            from person in _connection.PeopleExtended.LeftJoin(person => person.Id == orgGroup.Supervisor)
                .DefaultIfEmpty()
            select new OrgGroupWithSupervisor()
            {
                ApproverIsSupervisor = orgGroup.ApproverIsSupervisor,
                GroupName = orgGroup.GroupName,
                Id = orgGroup.Id,
                Supervisor = orgGroup.Supervisor,
                ParentId = orgGroup.ParentId,
                Type = orgGroup.Type,
                SupervisorPerson = person
            };
    }
}