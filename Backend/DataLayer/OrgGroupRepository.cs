using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Backend.Entities;
using LinqToDB;

namespace Backend.DataLayer
{
    public class OrgGroupRepository
    {
        private readonly IDbConnection _connection;
        private readonly PersonRepository _personRepository;

        public OrgGroupRepository(IDbConnection connection, PersonRepository personRepository)
        {
            _connection = connection;
            _personRepository = personRepository;
        }

        public IQueryable<OrgGroup> OrgGroups => _connection.OrgGroups;

        public IQueryable<OrgGroup> GetByIdWithChildren(Guid id)
        {
            return GetWithChildrenWhere(orgGroup => orgGroup.Id == id);
        }

        public IQueryable<OrgGroup> GetBySupervisorIdWithChildren(Guid supervisorId)
        {
            return GetWithChildrenWhere(orgGroup => orgGroup.Supervisor == supervisorId,
                childGroup => !childGroup.ApproverIsSupervisor);
        }

        public IQueryable<OrgGroup> GetWithChildrenWhere(Expression<Func<OrgGroup, bool>> rootPredicate,
            Expression<Func<OrgGroup, bool>> childPredicate = null)
        {
            return GetRecursive(OrgGroups.Where(rootPredicate),
                parents =>
                {
                    var childQuery = from child in OrgGroups
                        from parent in parents.InnerJoin(parent => parent.Id == child.ParentId)
                        select child;
                    if (childPredicate != null)
                        childQuery = childQuery.Where(childPredicate);
                    return childQuery;
                });
        }

        public delegate bool OrgGroupQueryPredicate(OrgGroup parent, OrgGroup child);

        public IQueryable<OrgGroup> GetWithParentsWhere(Expression<Func<OrgGroup, bool>> rootPredicate,
            Expression<OrgGroupQueryPredicate> parentQueryPredicate = null)
        {
            return GetWithParents(OrgGroups.Where(rootPredicate), parentQueryPredicate);
        }

        private IQueryable<OrgGroup> GetWithParents(IQueryable<OrgGroup> rootQuery,
            Expression<OrgGroupQueryPredicate> parentQueryPredicate = null)
        {
            return GetRecursive(rootQuery,
                children =>
                {
                    IQueryable<OrgGroup> parentQuery;
                    if (parentQueryPredicate != null)
                    {
                        parentQuery = from parent in OrgGroups
                            from child in children.InnerJoin(child => child.ParentId == parent.Id)
                            where parentQueryPredicate.Compile()(parent, child)
                            select parent;
                    }
                    else
                    {
                        parentQuery = from parent in OrgGroups
                            from child in children.InnerJoin(child => child.ParentId == parent.Id)
                            select parent;
                    }

                    return parentQuery;
                });
        }

        private IQueryable<OrgGroup> GetRecursive(IQueryable<OrgGroup> baseQuery,
            Func<IQueryable<OrgGroup>, IQueryable<OrgGroup>> subQueryFunc)
        {
            return _connection.GetCte<OrgGroup>(previous => baseQuery.Union(subQueryFunc(previous)));
        }

        public IQueryable<OrgGroupWithSupervisor> OrgGroupsWithSupervisor =>
            from orgGroup in OrgGroups
            from person in _personRepository.PeopleWithStaff
                .Where(staff => staff.StaffId != null)
                .LeftJoin(person => person.Id == orgGroup.Supervisor)
                .DefaultIfEmpty()
            select new OrgGroupWithSupervisor
            {
                ApproverIsSupervisor = orgGroup.ApproverIsSupervisor,
                GroupName = orgGroup.GroupName,
                Id = orgGroup.Id,
                Supervisor = orgGroup.Supervisor,
                ParentId = orgGroup.ParentId,
                Type = orgGroup.Type,
                SupervisorPerson = person
            };

        public IQueryable<OrgGroupWithSupervisor> StaffParentOrgGroups(Staff staff)
        {
            return from orgGroup in GetWithParentsWhere(g => staff.OrgGroupId == g.Id,
                    (parent, child) => !child.ApproverIsSupervisor || parent.ApproverIsSupervisor)
                from orgGroupWithSupervisor in OrgGroupsWithSupervisor.InnerJoin(g => g.Id == orgGroup.Id)
                where orgGroup.Supervisor != null
                select orgGroupWithSupervisor;
        }

        public List<OrgGroupWithSupervisor> GetOrgGroupsByPersonsRole(Guid personId)
        {
            var baseQuery = from role in _connection.PersonRoles
                from job in _connection.Job.InnerJoin(job => job.Id == role.JobId)
                from org in OrgGroups.InnerJoin(org => org.Id == job.OrgGroupId)
                where role.PersonId == personId && role.ActiveNow()
                select org;

            return (from org in GetWithParents(baseQuery, (parent, child) => child.Supervisor == null)
                from orgGroupWithSupervisor in OrgGroupsWithSupervisor.InnerJoin(g => g.Id == org.Id)
                where org.Supervisor != null
                select orgGroupWithSupervisor).ToList();
        }
    }
}